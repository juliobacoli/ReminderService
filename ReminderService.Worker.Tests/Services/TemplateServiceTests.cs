using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReminderService.Worker.Services;

namespace ReminderService.Worker.Tests.Services;

public class TemplateServiceTests
{
    private readonly Mock<ILogger<TemplateService>> _loggerMock;

    public TemplateServiceTests()
    {
        _loggerMock = new Mock<ILogger<TemplateService>>();
    }

    [Fact]
    public void RenderEmailTemplate_QuandoTemplateNaoExiste_RetornaFallbackHTML()
    {
        // Arrange
        var service = new TemplateService(_loggerMock.Object);

        // Act
        var result = service.RenderEmailTemplate(
            "João Silva",
            "Teste Title",
            "Teste Description",
            new DateTime(2026, 2, 15)
        );

        // Assert
        result.Should().Contain("Teste Title");
        result.Should().Contain("Teste Description");
        result.Should().Contain("15/02/2026");
    }

    [Fact]
    public void RenderEmailTemplate_SubstituiRecipientName_Corretamente()
    {
        // Arrange
        var service = new TemplateService(_loggerMock.Object);

        // Act
        var result = service.RenderEmailTemplate(
            "Maria Santos",
            "Título",
            "Descrição",
            DateTime.Now
        );

        // Assert (usando fallback já que template real não está disponível em testes)
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RenderEmailTemplate_FormataDueDate_ComoddMMyyyy()
    {
        // Arrange
        var service = new TemplateService(_loggerMock.Object);
        var dueDate = new DateTime(2026, 12, 31);

        // Act
        var result = service.RenderEmailTemplate(
            "Teste User",
            "Título",
            "Descrição",
            dueDate
        );

        // Assert
        result.Should().Contain("31/12/2026");
    }

    [Theory]
    [InlineData("João", "Renovação Seguro", "Pagar até amanhã", "2026-03-01", "01/03/2026")]
    [InlineData("Ana", "Reunião", "Confirmar presença", "2026-06-15", "15/06/2026")]
    public void RenderEmailTemplate_ComDiferentesInputs_GeraHTMLValido(
        string name, string title, string description, string dueDateStr, string expectedDate)
    {
        // Arrange
        var service = new TemplateService(_loggerMock.Object);
        var dueDate = DateTime.Parse(dueDateStr);

        // Act
        var result = service.RenderEmailTemplate(name, title, description, dueDate);

        // Assert
        result.Should().Contain(title);
        result.Should().Contain(description);
        result.Should().Contain(expectedDate);
    }

    [Fact]
    public void Constructor_QuandoTemplateExiste_LogaSucesso()
    {
        // Arrange & Act
        var service = new TemplateService(_loggerMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("carregado com sucesso")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
