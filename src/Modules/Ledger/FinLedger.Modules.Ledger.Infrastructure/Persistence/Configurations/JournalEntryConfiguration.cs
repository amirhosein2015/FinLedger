using FinLedger.Modules.Ledger.Domain.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinLedger.Modules.Ledger.Infrastructure.Persistence.Configurations;

internal class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(x => x.Id);

        // تنظیم رابطه یک به چند با Lines به صورت Owned Entity یا جداگانه
        // اینجا به صورت جداگانه تنظیم می‌کنیم
        builder.OwnsMany(x => x.Lines, line => 
        {
            line.ToTable("JournalEntryLines");
            line.WithOwner().HasForeignKey("JournalEntryId");
            line.Property(x => x.Debit).HasPrecision(18, 2);
            line.Property(x => x.Credit).HasPrecision(18, 2);
        });
    }
}
