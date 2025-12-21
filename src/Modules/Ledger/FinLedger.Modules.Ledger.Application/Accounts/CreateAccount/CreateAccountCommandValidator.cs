using FluentValidation;

namespace FinLedger.Modules.Ledger.Application.Accounts.CreateAccount;

// Principal Signal: قوانین ورودی را از منطق اجرا جدا کردیم
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد حساب نمی‌تواند خالی باشد.")
            .MaximumLength(20).WithMessage("کد حساب نباید بیشتر از ۲۰ کاراکتر باشد.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام حساب الزامی است.")
            .MinimumLength(3).WithMessage("نام حساب باید حداقل ۳ کاراکتر باشد.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع حساب انتخاب شده معتبر نیست.");
    }
}
