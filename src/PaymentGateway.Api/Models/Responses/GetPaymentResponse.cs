using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses;

public class GetPaymentResponse
{
    public Guid Id { get; set; }

    public PaymentStatus Status { get; set; }

    [JsonIgnore]
    public string CardNumber { get; set; } = string.Empty;

    private string? _cardNumberLastFour;

    public string CardNumberLastFour
    {
        get => _cardNumberLastFour ?? (CardNumber.Length >= 4 ? CardNumber[^4..] : string.Empty);
        set => _cardNumberLastFour = value;
    }

    public int ExpiryMonth { get; set; }

    public int ExpiryYear { get; set; }

    public string Currency { get; set; } = string.Empty;

    public int Amount { get; set; }
}