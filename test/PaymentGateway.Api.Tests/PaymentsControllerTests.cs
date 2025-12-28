using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PaymentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private CardPaymentRequest CreateValidPaymentRequest(string cardNumberEnding = "1")
    {
        var currentYear = DateTime.UtcNow.Year;
        return new CardPaymentRequest
        {
            CardNumber = $"123456789012345{cardNumberEnding}", // Last digit determines bank response
            ExpiryMonth = 12,
            ExpiryYear = currentYear + 1,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
    }

    #region ProcessPayment Tests - Authorized

    [Theory]
    [InlineData("1")]
    [InlineData("3")]
    [InlineData("5")]
    [InlineData("7")]
    [InlineData("9")]
    public async Task ProcessPayment_WhenValidRequestAndBankAuthorizes_ShouldReturnAuthorizedStatus(string cardNumberEnding)
    {
        // Arrange
        var request = CreateValidPaymentRequest(cardNumberEnding);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        var expectedLastFour = request.CardNumber.Length >= 4 ? request.CardNumber[^4..] : request.CardNumber;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse!.Status);
        Assert.Equal(expectedLastFour, paymentResponse.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(request.Currency, paymentResponse.Currency);
        Assert.Equal(request.Amount, paymentResponse.Amount);
        Assert.NotEqual(Guid.Empty, paymentResponse.Id);
    }

    #endregion

    #region ProcessPayment Tests - Declined

    [Theory]
    [InlineData("2")]
    [InlineData("4")]
    [InlineData("6")]
    [InlineData("8")]
    public async Task ProcessPayment_WhenValidRequestAndBankDeclines_ShouldReturnDeclinedStatus(string cardNumberEnding)
    {
        // Arrange
        var request = CreateValidPaymentRequest(cardNumberEnding);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        var expectedLastFour = request.CardNumber.Length >= 4 ? request.CardNumber[^4..] : request.CardNumber;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Declined, paymentResponse!.Status);
        Assert.Equal(expectedLastFour, paymentResponse.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(request.Currency, paymentResponse.Currency);
        Assert.Equal(request.Amount, paymentResponse.Amount);
        Assert.NotEqual(Guid.Empty, paymentResponse.Id);
    }

    [Fact]
    public async Task ProcessPayment_WhenCardNumberEndsWithZero_ShouldReturnDeclinedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("0");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Declined, paymentResponse!.Status);
    }

    #endregion

    #region ProcessPayment Tests - Rejected

    [Fact]
    public async Task ProcessPayment_WhenCardNumberIsEmpty_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        request.CardNumber = string.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenCardNumberIsInvalid_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        request.CardNumber = "123";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenExpiryYearIsInvalid_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        request.ExpiryYear = DateTime.UtcNow.Year - 1;

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenCurrencyIsInvalid_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        request.Currency = "XX";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenAmountIsZero_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        request.Amount = 0;

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenCvvIsInvalid_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        request.Cvv = "12";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenCardHasExpired_ShouldReturnRejectedStatus()
    {
        // Arrange
        var request = CreateValidPaymentRequest("1");
        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        request.ExpiryYear = lastMonth.Year;
        request.ExpiryMonth = lastMonth.Month;

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<CardPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    #endregion

    #region GetPayment Tests

    [Fact]
    public async Task GetPayment_WhenPaymentExists_ShouldReturnPaymentDetails()
    {
        // Arrange - First create a payment
        var request = CreateValidPaymentRequest("1");
        var createResponse = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<CardPaymentResponse>();
        Assert.NotNull(createdPayment);

        // Act - Retrieve the payment
        var getResponse = await _client.GetAsync($"/api/Payments/GetPayment/{createdPayment!.Id}");
        var paymentResponse = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(createdPayment.Id, paymentResponse!.Id);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse.Status);
        Assert.Equal("3451", paymentResponse.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(request.Currency, paymentResponse.Currency);
        Assert.Equal(request.Amount, paymentResponse.Amount);
    }

    [Fact]
    public async Task GetPayment_WhenPaymentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Payments/GetPayment/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_WhenPaymentExistsWithDeclinedStatus_ShouldReturnPaymentDetails()
    {
        // Arrange - Create a declined payment (even number ending)
        var request = CreateValidPaymentRequest("2");
        var createResponse = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<CardPaymentResponse>();
        Assert.NotNull(createdPayment);
        Assert.Equal(PaymentStatus.Declined, createdPayment!.Status);

        // Act - Retrieve the payment
        var getResponse = await _client.GetAsync($"/api/Payments/GetPayment/{createdPayment.Id}");
        var paymentResponse = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(createdPayment.Id, paymentResponse!.Id);
        Assert.Equal(PaymentStatus.Declined, paymentResponse.Status);
        Assert.Equal("3452", paymentResponse.CardNumberLastFour);
        Assert.Equal(request.Currency, paymentResponse.Currency);
        Assert.Equal(request.Amount, paymentResponse.Amount);
    }

    [Fact]
    public async Task GetPayment_WhenMultiplePaymentsExist_ShouldReturnCorrectPayment()
    {
        // Arrange - Create multiple payments
        var request1 = CreateValidPaymentRequest("1"); // Authorized
        var request2 = CreateValidPaymentRequest("2"); // Declined
        
        var createResponse1 = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request1);
        var createResponse2 = await _client.PostAsJsonAsync("/api/Payments/ProcessPayment", request2);
        
        var payment1 = await createResponse1.Content.ReadFromJsonAsync<CardPaymentResponse>();
        var payment2 = await createResponse2.Content.ReadFromJsonAsync<CardPaymentResponse>();
        
        Assert.NotNull(payment1);
        Assert.NotNull(payment2);

        // Act - Retrieve the second payment
        var getResponse = await _client.GetAsync($"/api/Payments/GetPayment/{payment2!.Id}");
        var paymentResponse = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(payment2.Id, paymentResponse!.Id);
        Assert.Equal(PaymentStatus.Declined, paymentResponse.Status);
        Assert.Equal("3452", paymentResponse.CardNumberLastFour);
        Assert.Equal(request2.Currency, paymentResponse.Currency);
        Assert.Equal(request2.Amount, paymentResponse.Amount);
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}
