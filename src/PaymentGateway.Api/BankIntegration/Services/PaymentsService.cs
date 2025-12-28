using Newtonsoft.Json;
using PaymentGateway.Api.BankIntegration.Models;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.BankIntegration.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentsService> _logger;

        public PaymentsService(HttpClient httpClient, ILogger<PaymentsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> ProcessPaymentAsync(CardPaymentRequest cardPaymentRequest, CancellationToken cancellationToken = default)
        {
            bool result = false;

            try
            {
                PaymentRequest paymentRequest = PrepareRequest(cardPaymentRequest);
                _logger.LogInformation("Bank service payment request. {Request}", JsonConvert.SerializeObject(paymentRequest));

                var response = await _httpClient.PostAsJsonAsync("/payments", paymentRequest, cancellationToken);

                _logger.LogInformation("Bank service payment response. {Response}", JsonConvert.SerializeObject(response));

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var bankResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken);
                    result = bankResponse?.Authorized ?? false;
                }
                else
                {
                    ResponseHelper.HandleUnsuccessfulResponse(response, _logger);
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occured when calling bank service. {Request}.", JsonConvert.SerializeObject(cardPaymentRequest));
            }

            return result;
        }

        private PaymentRequest PrepareRequest(CardPaymentRequest cardPaymentRequest)
        {
            return new PaymentRequest
            {
                CardNumber = cardPaymentRequest.CardNumber,
                ExpiryDate = $"{cardPaymentRequest.ExpiryMonth:D2}/{cardPaymentRequest.ExpiryYear}",
                Currency = cardPaymentRequest.Currency,
                Amount = cardPaymentRequest.Amount,
                Cvv = cardPaymentRequest.Cvv
            };
        }
    }
}
