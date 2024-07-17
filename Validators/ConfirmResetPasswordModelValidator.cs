using FluentValidation;
using Feast.Models;

namespace Feast.Validators;
public class ConfirmResetPasswordModelValidator : AbstractValidator<ConfirmResetPasswordModel>
{
    public ConfirmResetPasswordModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
    }
}