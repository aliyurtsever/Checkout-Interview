using PaymentGateway.Api.BankIntegration.Services;
using PaymentGateway.Api.Mapping;
using PaymentGateway.Api.Services;
using AutoMapper;
using FluentValidation;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAutoMapper(typeof(PaymentMappingProfile).Assembly);

        var bankServiceBaseUrl = builder.Configuration["BankService:BaseUrl"] ?? "http://localhost:8080";
        builder.Services.AddHttpClient<IPaymentsService, PaymentsService>(client =>
        {
            client.BaseAddress = new Uri(bankServiceBaseUrl);
        });

        builder.Services.AddSingleton<PaymentsRepository>();
        builder.Services.AddScoped<IValidator<CardPaymentRequest>, CardPaymentRequestValidator>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
