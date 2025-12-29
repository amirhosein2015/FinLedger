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

        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        
        // Ensure Status is stored as an integer in the DB
        builder.Property(x => x.Status).HasConversion<int>();

        // Explicitly configure the collection of lines
        builder.OwnsMany(x => x.Lines, line => 
        {
            line.ToTable("JournalEntryLines");
            line.HasKey(x => x.Id);
            
            //  Tell EF Core that we generate Guids in C#, NOT the Database
            line.Property(x => x.Id).ValueGeneratedNever(); 
            
            line.WithOwner().HasForeignKey(x => x.JournalEntryId);
            
            line.Property(x => x.Debit).HasPrecision(18, 2);
            line.Property(x => x.Credit).HasPrecision(18, 2);
        });
    }
}
