namespace PaymentGateway.Api.Constants
{
    public static class ErrorMessages
    {
        public static readonly string CardNumberIsRequired = "Card number is required.";
        public static readonly string CardNumberLengthInvalid = "Card number must be between 14-19 digits.";
        public static readonly string CardNumberMustBeNumeric = "Card number must be numeric.";
        public static readonly string ExpiryMonthInvalid = "Expiry month must be between 1 and 12.";
        public static readonly string ExpiryYearInvalid = "Expiry year must be a valid year.";
        public static readonly string CurrencyIsRequired = "Currency is required.";
        public static readonly string CurrencyMustBeISOCode = "Currency must be a 3-letter ISO code (e.g., GBP, USD, EUR).";
        public static readonly string AmountMustBePositive = "Amount must be greater than 0.";
        public static readonly string CvvIsRequired = "CVV is required.";
        public static readonly string CvvMustBe3Or4Digits = "CVV must be 3 or 4 digits.";
        public static readonly string CardHasExpired = "Card has expired.";
    }
}
