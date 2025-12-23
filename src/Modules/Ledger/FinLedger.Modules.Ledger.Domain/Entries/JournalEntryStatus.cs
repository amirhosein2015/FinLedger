namespace FinLedger.Modules.Ledger.Domain.Entries;

public enum JournalEntryStatus
{
    Draft = 1,
    Posted = 2,
    Reversed = 3,
    Reversal = 4 // This entry was created to reverse another one
}
