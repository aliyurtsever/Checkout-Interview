using FluentValidation;
using PaymentGateway.Api.Constants;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators
{
    public class CardPaymentRequestValidator : AbstractValidator<CardPaymentRequest>
    {
        public CardPaymentRequestValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage(ErrorMessages.CardNumberIsRequired)
                .Length(14, 19)
                .WithMessage(ErrorMessages.CardNumberLengthInvalid)
                .Matches(@"^\d+$")
                .WithMessage(ErrorMessages.CardNumberMustBeNumeric);

            RuleFor(x => x.ExpiryMonth)
                .InclusiveBetween(1, 12)
                .WithMessage(ErrorMessages.ExpiryMonthInvalid);

            int currentYear = DateTime.UtcNow.Year;
            RuleFor(x => x.ExpiryYear)
                .InclusiveBetween(currentYear, currentYear + 20)
                .WithMessage(ErrorMessages.ExpiryYearInvalid);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage(ErrorMessages.CurrencyIsRequired)
                .Matches(@"^[A-Z]{3}$")
                .WithMessage(ErrorMessages.CurrencyMustBeISOCode);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.AmountMustBePositive);

            RuleFor(x => x.Cvv)
                .NotEmpty()
                .WithMessage(ErrorMessages.CvvIsRequired)
                .Matches(@"^\d{3,4}$")
                .WithMessage(ErrorMessages.CvvMustBe3Or4Digits);

            RuleFor(x => x)
                .Must(x => IsExpiryDateValid(x.ExpiryYear, x.ExpiryMonth))
                .WithMessage(ErrorMessages.CardHasExpired)
                .OverridePropertyName("ExpiryDate");
        }

        private static bool IsExpiryDateValid(int year, int month)
        {
            if (month < 1 || month > 12)
                return false;

            var now = DateTime.UtcNow;

            var lastDayOfMonth = DateTime.DaysInMonth(year, month);
            var expiryDate = new DateTime(year, month, lastDayOfMonth, 23, 59, 59, DateTimeKind.Utc);

            return now <= expiryDate;
        }
    }
}
