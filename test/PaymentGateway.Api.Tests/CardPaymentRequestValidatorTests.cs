using PaymentGateway.Api.Constants;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests;

public class CardPaymentRequestValidatorTests
{
    private readonly CardPaymentRequestValidator _validator;
    private readonly int _currentYear;

    public CardPaymentRequestValidatorTests()
    {
        _validator = new CardPaymentRequestValidator();
        _currentYear = DateTime.UtcNow.Year;
    }

    private CardPaymentRequest CreateValidRequest()
    {
        return new CardPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = _currentYear + 1,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
    }

    #region CardNumber Tests

    [Fact]
    public void Validate_WhenCardNumberIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber" && e.ErrorMessage == ErrorMessages.CardNumberIsRequired);
    }

    [Fact]
    public void Validate_WhenCardNumberIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = null!;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber" && e.ErrorMessage == ErrorMessages.CardNumberIsRequired);
    }

    [Fact]
    public void Validate_WhenCardNumberIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = "1234567890123"; // 13 digits

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber" && e.ErrorMessage == ErrorMessages.CardNumberLengthInvalid);
    }

    [Fact]
    public void Validate_WhenCardNumberIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = "12345678901234567890"; // 20 digits

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber" && e.ErrorMessage == ErrorMessages.CardNumberLengthInvalid);
    }

    [Fact]
    public void Validate_WhenCardNumberContainsNonNumericCharacters_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = "123456789012345A";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber" && e.ErrorMessage == ErrorMessages.CardNumberMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenCardNumberContainsSpaces_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = "1234 5678 9012 3456";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber" && e.ErrorMessage == ErrorMessages.CardNumberMustBeNumeric);
    }

    [Fact]
    public void Validate_WhenCardNumberIsValid14Digits_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = "12345678901234";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "CardNumber");
    }

    [Fact]
    public void Validate_WhenCardNumberIsValid19Digits_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = "1234567890123456789";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "CardNumber");
    }

    #endregion

    #region ExpiryMonth Tests

    [Fact]
    public void Validate_WhenExpiryMonthIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = 0;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }

    [Fact]
    public void Validate_WhenExpiryMonthIsGreaterThan12_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = 13;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }

    [Fact]
    public void Validate_WhenExpiryMonthIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = 6;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }

    [Fact]
    public void Validate_WhenExpiryMonthIs1_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = 1;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }

    [Fact]
    public void Validate_WhenExpiryMonthIs12_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = 12;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }

    #endregion

    #region ExpiryYear Tests

    [Fact]
    public void Validate_WhenExpiryYearIsTooOld_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryYear = _currentYear - 1;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryYear" && e.ErrorMessage == ErrorMessages.ExpiryYearInvalid);
    }

    [Fact]
    public void Validate_WhenExpiryYearIsTooFarInFuture_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryYear = _currentYear + 21;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryYear" && e.ErrorMessage == ErrorMessages.ExpiryYearInvalid);
    }

    [Fact]
    public void Validate_WhenExpiryYearIsCurrentYear_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        var currentDate = DateTime.UtcNow;
        request.ExpiryYear = _currentYear;
        // Ensure expiry month is in the future to avoid expiry date validation failure
        // If current month is December, use December (which should be valid if we're early in the month)
        // Otherwise, use next month
        if (currentDate.Month == 12)
        {
            request.ExpiryMonth = 12; // December - valid if card hasn't expired yet this month
        }
        else
        {
            request.ExpiryMonth = currentDate.Month + 1; // Next month
        }

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ExpiryYear");
    }

    [Fact]
    public void Validate_WhenExpiryYearIs20YearsInFuture_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryYear = _currentYear + 20;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ExpiryYear");
    }

    #endregion

    #region Currency Tests

    [Fact]
    public void Validate_WhenCurrencyIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == ErrorMessages.CurrencyIsRequired);
    }

    [Fact]
    public void Validate_WhenCurrencyIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = null!;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == ErrorMessages.CurrencyIsRequired);
    }

    [Fact]
    public void Validate_WhenCurrencyIsNot3Letters_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = "GB";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == ErrorMessages.CurrencyMustBeISOCode);
    }

    [Fact]
    public void Validate_WhenCurrencyIs4Letters_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = "GBPP";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == ErrorMessages.CurrencyMustBeISOCode);
    }

    [Fact]
    public void Validate_WhenCurrencyContainsLowercaseLetters_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = "gbp";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == ErrorMessages.CurrencyMustBeISOCode);
    }

    [Fact]
    public void Validate_WhenCurrencyContainsNumbers_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = "GB1";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == ErrorMessages.CurrencyMustBeISOCode);
    }

    [Fact]
    public void Validate_WhenCurrencyIsValidISO_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = "USD";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Validate_WhenCurrencyIsEUR_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = "EUR";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Currency");
    }

    #endregion

    #region Amount Tests

    [Fact]
    public void Validate_WhenAmountIsZero_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = 0;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.ErrorMessage == ErrorMessages.AmountMustBePositive);
    }

    [Fact]
    public void Validate_WhenAmountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = -100;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.ErrorMessage == ErrorMessages.AmountMustBePositive);
    }

    [Fact]
    public void Validate_WhenAmountIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = 1;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_WhenAmountIsLarge_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = 999999;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Amount");
    }

    #endregion

    #region CVV Tests

    [Fact]
    public void Validate_WhenCvvIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv" && e.ErrorMessage == ErrorMessages.CvvIsRequired);
    }

    [Fact]
    public void Validate_WhenCvvIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = null!;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv" && e.ErrorMessage == ErrorMessages.CvvIsRequired);
    }

    [Fact]
    public void Validate_WhenCvvIsLessThan3Digits_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = "12";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv" && e.ErrorMessage == ErrorMessages.CvvMustBe3Or4Digits);
    }

    [Fact]
    public void Validate_WhenCvvIsMoreThan4Digits_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = "12345";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv" && e.ErrorMessage == ErrorMessages.CvvMustBe3Or4Digits);
    }

    [Fact]
    public void Validate_WhenCvvContainsNonNumericCharacters_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = "12A";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv" && e.ErrorMessage == ErrorMessages.CvvMustBe3Or4Digits);
    }

    [Fact]
    public void Validate_WhenCvvIs3Digits_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = "123";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Cvv");
    }

    [Fact]
    public void Validate_WhenCvvIs4Digits_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = "1234";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Cvv");
    }

    #endregion

    #region ExpiryDate Tests

    [Fact]
    public void Validate_WhenCardHasExpired_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        var currentDate = DateTime.UtcNow;
        request.ExpiryYear = currentDate.Year - 1;
        request.ExpiryMonth = currentDate.Month;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryDate" && e.ErrorMessage == ErrorMessages.CardHasExpired);
    }

    [Fact]
    public void Validate_WhenCardExpiresNextMonth_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        var currentDate = DateTime.UtcNow;
        var nextMonth = currentDate.AddMonths(1);
        request.ExpiryYear = nextMonth.Year;
        request.ExpiryMonth = nextMonth.Month;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ExpiryDate");
    }

    [Fact]
    public void Validate_WhenCardExpiredLastMonth_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        request.ExpiryYear = lastMonth.Year;
        request.ExpiryMonth = lastMonth.Month;

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryDate" && e.ErrorMessage == ErrorMessages.CardHasExpired);
    }

    #endregion

    #region Valid Request Tests

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var request = new CardPaymentRequest
        {
            CardNumber = "123", // too short
            ExpiryMonth = 12, // valid month (to avoid DateTime exception)
            ExpiryYear = 2000, // too old
            Currency = "XX", // invalid
            Amount = -10, // negative
            Cvv = "12" // too short
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber");
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryYear");
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency");
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv");
    }

    #endregion
}

