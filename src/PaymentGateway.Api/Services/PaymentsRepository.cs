using PaymentGateway.Api.Models.Repository;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    private List<Payment> Payments = new();
    
    public void Add(Payment payment)
    {
        Payments.Add(payment);
    }

    public Task<Payment?> GetAsync(Guid id)
    {
        return Task.FromResult(Payments.FirstOrDefault(p => p.Id == id));
    }
}