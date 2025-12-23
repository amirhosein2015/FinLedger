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

    public static JournalEntry Create(DateTime date, string description, List<(Guid AccountId, decimal Debit, decimal Credit)> lines)
    {
        if (lines == null || lines.Count < 2)
            throw new ArgumentException("A journal entry must have at least two lines.");

        var entry = new JournalEntry
        {
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

    public void Post()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Only draft entries can be posted.");

        ValidateBalance();
        Status = JournalEntryStatus.Posted;
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
            throw new InvalidOperationException($"Out of balance. Debit: {totalDebit}, Credit: {totalCredit}");
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

    // Private constructor for EF Core
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
