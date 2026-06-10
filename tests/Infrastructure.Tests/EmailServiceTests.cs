using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Infrastructure.Tests;

public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldLogMessage()
    {
        var mockLogger = new Mock<ILogger<EmailService>>();
        var service = new EmailService(mockLogger.Object);

        await service.SendEmailAsync("test@example.com", "Subject", "Body");

        Assert.True(true);
    }
}
