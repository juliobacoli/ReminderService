using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ReminderService.Worker.Services;

namespace ReminderService.Worker.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<EmailService>> _loggerMock;

    public EmailServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
    }

    [Fact]
    public async Task SendEmailAsync_QuandoUsernameVazio_RetornaFalse()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:Username"]).Returns(string.Empty);
        _configurationMock.Setup(c => c["EmailSettings:Password"]).Returns("senha-valida");

        var service = new EmailService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await service.SendEmailAsync(
            "teste@email.com",
            "Teste User",
            "Assunto",
            "<html>Corpo</html>"
        );

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Credenciais SMTP não configuradas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_QuandoPasswordVazio_RetornaFalse()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:Username"]).Returns("usuario-valido");
        _configurationMock.Setup(c => c["EmailSettings:Password"]).Returns(string.Empty);

        var service = new EmailService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await service.SendEmailAsync(
            "teste@email.com",
            "Teste User",
            "Assunto",
            "<html>Corpo</html>"
        );

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Credenciais SMTP não configuradas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_QuandoAmbosVazios_RetornaFalse()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:Username"]).Returns(string.Empty);
        _configurationMock.Setup(c => c["EmailSettings:Password"]).Returns(string.Empty);

        var service = new EmailService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await service.SendEmailAsync(
            "teste@email.com",
            "Teste User",
            "Assunto",
            "<html>Corpo</html>"
        );

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Credenciais SMTP não configuradas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
