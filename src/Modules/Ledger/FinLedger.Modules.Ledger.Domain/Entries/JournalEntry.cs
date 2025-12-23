using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Domain.Entries;

/// <summary>
/// Aggregate root representing a financial journal entry.
/// Enforces accounting invariants and immutability rules.
/// </summary>
public class JournalEntry : AggregateRoot
{
    public DateTime TransactionDate { get; private set; }
    public string Description { get; private set; } = default!;
    public List<JournalEntryLine> Lines { get; private set; } = new();
    public JournalEntryStatus Status { get; private set; }

    private JournalEntry() { }

    /// <summary>
    /// Factory method to create a new journal entry in Draft status.
    /// </summary>
    public static JournalEntry Create(DateTime date, string description, List<(Guid AccountId, decimal Debit, decimal Credit)> lines)
    {
        if (lines == null || lines.Count < 2)
            throw new ArgumentException("A journal entry must have at least two lines.");

        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            TransactionDate = date,
            Description = description,
            Status = JournalEntryStatus.Draft
        };

        foreach (var line in lines)
        {
            entry.InternalAddLine(line.AccountId, line.Debit, line.Credit);
        }

        entry.ValidateBalance();
        return entry;
    }

    /// <summary>
    /// Finalizes the journal entry. Once posted, it cannot be modified or deleted.
    /// </summary>
    public void Post()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Only draft entries can be posted.");

        ValidateBalance();
        Status = JournalEntryStatus.Posted;
    }

    /// <summary>
    /// Creates a counter-entry to reverse the effects of a posted entry.
    /// This is the only way to "undo" a transaction in a professional ledger.
    /// </summary>
    public JournalEntry CreateReversal(string reason)
    {
        if (Status != JournalEntryStatus.Posted)
            throw new InvalidOperationException("Only posted entries can be reversed.");

        // Create a list of lines with flipped Debit and Credit amounts
        var reversalLines = Lines
            .Select(l => (l.AccountId, l.Credit, l.Debit))
            .ToList();
        
        var reversalDescription = $"Reversal of Entry {Id}: {reason}";
        var reversalEntry = Create(DateTime.UtcNow, reversalDescription, reversalLines);
        
        // Mark this new entry as a Reversal type
        reversalEntry.Status = JournalEntryStatus.Reversal;

        // Update the original entry's status to indicate it has been reversed
        this.Status = JournalEntryStatus.Reversed;
        
        return reversalEntry;
    }

    private void InternalAddLine(Guid accountId, decimal debit, decimal credit)
    {
        if (debit != 0 && credit != 0)
            throw new ArgumentException("A single line cannot have both Debit and Credit values.");
        
        Lines.Add(new JournalEntryLine(Id, accountId, debit, credit));
    }

    private void ValidateBalance()
    {
        var totalDebit = Lines.Sum(x => x.Debit);
        var totalCredit = Lines.Sum(x => x.Credit);

        if (totalDebit != totalCredit)
            throw new InvalidOperationException($"Journal entry is out of balance. Total Debit: {totalDebit}, Total Credit: {totalCredit}");
    }
}

/// <summary>
/// Represents a single debit or credit line within a journal entry.
/// </summary>
public class JournalEntryLine
{
    public Guid Id { get; private set; }
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    private JournalEntryLine() { }

    public JournalEntryLine(Guid journalEntryId, Guid accountId, decimal debit, decimal credit)
    {
        Id = Guid.NewGuid();
        JournalEntryId = journalEntryId;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
    }
}

