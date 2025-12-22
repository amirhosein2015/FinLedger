using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Domain.Entries;

public class JournalEntry : AggregateRoot
{
    public DateTime TransactionDate { get; private set; }
    public string Description { get; private set; } = default!;
    public List<JournalEntryLine> Lines { get; private set; } = new();
    public bool IsPosted { get; private set; }

    private JournalEntry() { }

    public static JournalEntry Create(DateTime date, string description, List<(Guid AccountId, decimal Debit, decimal Credit)> lines)
    {
        // Principal Signal: Validation inside Domain (Rich Domain Model)
        if (lines == null || lines.Count < 2)
            throw new ArgumentException("A journal entry must have at least two lines.");

        var entry = new JournalEntry
        {
            TransactionDate = date,
            Description = description,
            IsPosted = false
        };

        foreach (var line in lines)
        {
            entry.AddLine(line.AccountId, line.Debit, line.Credit);
        }

        entry.ValidateBalance();
        return entry;
    }

    private void AddLine(Guid accountId, decimal debit, decimal credit)
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

public class JournalEntryLine
{
    public Guid Id { get; private set; }
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    public JournalEntryLine(Guid journalEntryId, Guid accountId, decimal debit, decimal credit)
    {
        Id = Guid.NewGuid();
        JournalEntryId = journalEntryId;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
    }
}
