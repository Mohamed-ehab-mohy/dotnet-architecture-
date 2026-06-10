using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PaymentGatewayService
{
    private readonly ILogger<PaymentGatewayService> _logger;

    public PaymentGatewayService(ILogger<PaymentGatewayService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(string cardNumber, decimal amount)
    {
        _logger.LogInformation("Processing payment of {Amount}", amount);

        await Task.CompletedTask;

        return true;
    }

    public async Task<bool> RefundPaymentAsync(string transactionId)
    {
        _logger.LogInformation("Refunding transaction: {TransactionId}", transactionId);

        await Task.CompletedTask;

        return true;
    }
}
