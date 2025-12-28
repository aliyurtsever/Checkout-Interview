using System.Net;

namespace PaymentGateway.Api.Helpers
{
    public static class ResponseHelper
    {
        public static void HandleUnsuccessfulResponse(HttpResponseMessage response, ILogger logger)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    logger.LogWarning("Required fields is missing from the request.StatusCode: {StatusCode}", response.StatusCode);
                    break;
                case HttpStatusCode.ServiceUnavailable:
                    logger.LogError("Bank service unavailable.{StatusCode}", response.StatusCode);
                    break;
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    logger.LogError("Authorization error from bank service. StatusCode: {StatusCode}", response.StatusCode);
                    break;
                case HttpStatusCode.NotFound:
                    logger.LogWarning("Bank payment endpoint not found.{StatusCode}", response.StatusCode);
                    break;
                case HttpStatusCode.InternalServerError:
                    logger.LogError("Internal server error from bank service.{StatusCode}", response.StatusCode);
                    break;
                default:
                    logger.LogError("Unexpected status code from bank service: {StatusCode}", response.StatusCode);
                    break;
            }
        }
    }
}
