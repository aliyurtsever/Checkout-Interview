using AutoMapper;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Repository;

namespace PaymentGateway.Api.Mapping
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Payment, GetPaymentResponse>();
            CreateMap<CardPaymentResponse, Payment>();
        }
    }
}
