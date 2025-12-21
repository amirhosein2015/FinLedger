using FinLedger.BuildingBlocks.Domain;

namespace FinLedger.Modules.Ledger.Domain.Entries;

public class JournalEntry : AggregateRoot
{
    public DateTime TransactionDate { get; private set; }
    public string Description { get; private set; } = default!;
    public List<JournalEntryLine> Lines { get; private set; } = new();
    public bool IsPosted { get; private set; }

    private JournalEntry() { }

    public static JournalEntry Create(DateTime date, string description)
    {
        return new JournalEntry 
        { 
            TransactionDate = date, 
            Description = description,
            IsPosted = false 
        };
    }

    public void AddLine(Guid accountId, decimal debit, decimal credit)
    {
        if (IsPosted) throw new InvalidOperationException("سند تایید شده قابل تغییر نیست.");
        if (debit != 0 && credit != 0) throw new ArgumentException("یک سطر نمی‌تواند همزمان بدهکار و بستانکار باشد.");
        
        Lines.Add(new JournalEntryLine(accountId, debit, credit));
    }

    public void Post()
    {
        // قانون اصلی حسابداری: مجموع بدهکار = مجموع بستانکار
        var totalDebit = Lines.Sum(x => x.Debit);
        var totalCredit = Lines.Sum(x => x.Credit);

        if (totalDebit != totalCredit)
            throw new InvalidOperationException("سند تراز نیست! مجموع بدهکار و بستانکار باید برابر باشد.");

        IsPosted = true;
    }
}

public class JournalEntryLine // این یک Entity ساده است
{
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    public JournalEntryLine(Guid accountId, decimal debit, decimal credit)
    {
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
    }
}
