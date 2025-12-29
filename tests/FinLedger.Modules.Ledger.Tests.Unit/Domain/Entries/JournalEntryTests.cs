using FinLedger.Modules.Ledger.Domain.Entries;
using FluentAssertions;
using Xunit;

namespace FinLedger.Modules.Ledger.Tests.Unit.Domain.Entries;

public class JournalEntryTests
{
    private readonly Guid _cashAccountId = Guid.NewGuid();
    private readonly Guid _bankAccountId = Guid.NewGuid();

    [Fact]
    public void Create_Should_Throw_Exception_When_Lines_Are_Unbalanced()
    {
        // Arrange: ایجاد دو ردیف که تراز نیستند (۱۰۰۰ در مقابل ۹۵۰)
        var lines = new List<(Guid AccountId, decimal Debit, decimal Credit)>
        {
            (_cashAccountId, 1000, 0), 
            (_bankAccountId, 0, 950)   
        };

        // Act: تلاش برای ساخت سند
        Action act = () => JournalEntry.Create(DateTime.UtcNow, "Unbalanced Entry", lines);

        // Assert: باید خطا پرتاب شود و متن خطا شامل عبارت "balance" باشد
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*balance*");
    }

    [Fact]
    public void Create_Should_Succeed_When_Lines_Are_Balanced()
    {
        // Arrange: ایجاد ردیف‌های تراز (۱۵۰۰ = ۱۵۰۰)
        var lines = new List<(Guid AccountId, decimal Debit, decimal Credit)>
        {
            (_cashAccountId, 1500, 0),
            (_bankAccountId, 0, 1500)
        };

        // Act
        var entry = JournalEntry.Create(DateTime.UtcNow, "Balanced Entry", lines);

        // Assert
        entry.Should().NotBeNull();
        entry.Status.Should().Be(JournalEntryStatus.Draft);
        entry.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Post_Should_Change_Status_To_Posted_When_Entry_Is_Draft()
    {
        // Arrange
        var lines = new List<(Guid AccountId, decimal Debit, decimal Credit)>
        {
            (_cashAccountId, 500, 0),
            (_bankAccountId, 0, 500)
        };
        var entry = JournalEntry.Create(DateTime.UtcNow, "To be posted", lines);

        // Act
        entry.Post();

        // Assert
        entry.Status.Should().Be(JournalEntryStatus.Posted);
    }

    [Fact]
    public void CreateReversal_Should_Flip_Debits_And_Credits()
    {
        // Arrange: سند اصلی (بدهکار نقد، بستانکار بانک)
        var lines = new List<(Guid AccountId, decimal Debit, decimal Credit)>
        {
            (_cashAccountId, 2000, 0), 
            (_bankAccountId, 0, 2000)  
        };
        var entry = JournalEntry.Create(DateTime.UtcNow, "Original Transaction", lines);
        entry.Post();

        // Act: ایجاد سند معکوس
        var reversal = entry.CreateReversal("Correction");

        // Assert: بررسی جابجایی دقیق مبالغ
        var cashLine = reversal.Lines.First(l => l.AccountId == _cashAccountId);
        var bankLine = reversal.Lines.First(l => l.AccountId == _bankAccountId);

        cashLine.Credit.Should().Be(2000); // بدهکارِ قدیم حالا باید بستانکار باشد
        bankLine.Debit.Should().Be(2000);  // بستانکارِ قدیم حالا باید بدهکار باشد
        
        reversal.Status.Should().Be(JournalEntryStatus.Reversal);
        entry.Status.Should().Be(JournalEntryStatus.Reversed);
    }
}
