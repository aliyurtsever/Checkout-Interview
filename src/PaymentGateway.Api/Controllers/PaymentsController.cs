using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;
using Newtonsoft.Json;
using PaymentGateway.Api.BankIntegration.Services;
using AutoMapper;
using PaymentGateway.Api.Models.Repository;
using FluentValidation;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IPaymentsService _paymentsService;
    private readonly IMapper _mapper;
    private readonly IValidator<CardPaymentRequest> _validator;

    public PaymentsController(PaymentsRepository paymentsRepository, ILogger<PaymentsController> logger, IPaymentsService paymentsService, IMapper mapper, IValidator<CardPaymentRequest> validator)
    {
        _paymentsRepository = paymentsRepository;
        _logger = logger;
        _paymentsService = paymentsService;
        _mapper = mapper;
        _validator = validator;
    }

    /// <summary>
    /// Processes a payment through the payment gateway
    /// </summary>
    /// <param name="cardPaymentRequest">Payment request details</param>
    /// <returns>The payment details including status, amount, currency, and card info</returns>
    [HttpPost("ProcessPayment")]
    public async Task<ActionResult<CardPaymentResponse?>> ProcessPaymentAsync([FromBody] CardPaymentRequest cardPaymentRequest)
    {
        _logger.LogInformation("Card Payment Request : {Request}", JsonConvert.SerializeObject(cardPaymentRequest));

        var cardPaymentRequestValidatorResult = await _validator.ValidateAsync(cardPaymentRequest);

        PaymentStatus status;
        if (!cardPaymentRequestValidatorResult.IsValid)
        {
            _logger.LogInformation("Card payment request is not valid. {Details}", JsonConvert.SerializeObject(cardPaymentRequestValidatorResult));
            status = PaymentStatus.Rejected;
        }
        else
        {
            bool result = await _paymentsService.ProcessPaymentAsync(cardPaymentRequest);
            status = result ? PaymentStatus.Authorized : PaymentStatus.Declined;
        }

        var paymentId = Guid.NewGuid();
        var paymentResponse = new CardPaymentResponse
        {
            Id = paymentId,
            Status = status,
            CardNumber = cardPaymentRequest.CardNumber,
            ExpiryMonth = cardPaymentRequest.ExpiryMonth,
            ExpiryYear = cardPaymentRequest.ExpiryYear,
            Currency = cardPaymentRequest.Currency,
            Amount = cardPaymentRequest.Amount
        };

        if (paymentResponse.Status != PaymentStatus.Rejected)
        {
            var payment = _mapper.Map<Payment>(paymentResponse);
            _paymentsRepository.Add(payment);
        }

        _logger.LogInformation("Card Payment Response : {Response}", JsonConvert.SerializeObject(paymentResponse));
        return new OkObjectResult(paymentResponse);
    }

    /// <summary>
    /// Retrieves a payment by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>The payment details including status, amount, currency, and card info.</returns>
    [HttpGet("GetPayment/{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Get Payment Request : {Id}", id);

            var payment = await _paymentsRepository.GetAsync(id);

            if (payment is null)
            {
                _logger.LogInformation("Payment could not found for : {Id}", id);
                return NotFound();
            }

            var response = _mapper.Map<GetPaymentResponse>(payment);
            _logger.LogInformation("Get Payment Response : {Response}", JsonConvert.SerializeObject(response));
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured when retrieving payment detail. {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving the payment detail.");
        }
    }
}