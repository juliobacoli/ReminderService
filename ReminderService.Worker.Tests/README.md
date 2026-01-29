# ReminderService.Worker.Tests

Testes unitários para o ReminderService Worker.

## Cobertura

### Services (10 testes)

#### TemplateServiceTests (5 testes)
- ✅ `RenderEmailTemplate_QuandoTemplateNaoExiste_RetornaFallbackHTML`
- ✅ `RenderEmailTemplate_SubstituiRecipientName_Corretamente`
- ✅ `RenderEmailTemplate_FormataDueDate_ComoddMMyyyy`
- ✅ `RenderEmailTemplate_ComDiferentesInputs_GeraHTMLValido` (2 cenários)

#### ReminderProcessingServiceTests (6 testes)
- ✅ `ProcessPendingRemindersAsync_QuandoNaoTemLembretes_NaoProcessaNada`
- ✅ `ProcessPendingRemindersAsync_AtualizaLastSentAt_ApenasSeEnviarComSucesso`
- ✅ `ProcessPendingRemindersAsync_NaoAtualizaLastSentAt_QuandoTodosEnviosFalham`
- ✅ `ProcessPendingRemindersAsync_ProcessaApenasDestinatariosAtivos`
- ✅ `ProcessPendingRemindersAsync_AdicionaPrefixoUrgente_QuandoPrioridadeHigh`
- ✅ `ProcessPendingRemindersAsync_ResetaStatusAntesDeProcesar`
- ✅ `ProcessPendingRemindersAsync_AtualizaStatusIndividual_ParaCadaDestinatario`

### Repositories (9 testes)

#### ReminderRepositoryTests (9 testes)
- ✅ `GetPendingRemindersAsync_QuandoNaoTemLastSentAt_Retorna`
- ✅ `GetPendingRemindersAsync_QuandoIntervaloNaoPassou_NaoRetorna`
- ✅ `GetPendingRemindersAsync_QuandoIntervaloPassou_Retorna`
- ✅ `GetPendingRemindersAsync_FiltrarApenasDestinatariosAtivos`
- ✅ `GetPendingRemindersAsync_NaoRetornaLembretesExpirados`
- ✅ `ResetReminderRecipientsStatusAsync_ResetaStatusParaPending`
- ✅ `UpdateReminderRecipientStatusAsync_AtualizaStatusParaSent`
- ✅ `UpdateReminderLastSentAtAsync_AtualizaDataCorretamente`

## Executar Testes

```bash
# Todos os testes
dotnet test

# Com verbosidade
dotnet test --verbosity normal

# Com cobertura de código
dotnet test /p:CollectCoverage=true
```

## Estrutura

```
ReminderService.Worker.Tests/
├── Services/
│   ├── TemplateServiceTests.cs
│   └── ReminderProcessingServiceTests.cs
├── Repositories/
│   └── ReminderRepositoryTests.cs
└── Helpers/
    └── TestDbContextFactory.cs
```

## Ferramentas Utilizadas

- **xUnit**: Framework de testes
- **Moq**: Mocking de interfaces
- **FluentAssertions**: Assertions expressivas
- **EF Core InMemory**: Banco de dados em memória para testes de repository

## Regras de Negócio Testadas

### 1. Cálculo de Lembretes Pendentes
- ✅ Retorna quando LastSentAt = null
- ✅ Retorna quando intervalo passou
- ✅ Não retorna quando intervalo não passou
- ✅ Não retorna lembretes expirados (DueDate < hoje)

### 2. Filtros de Destinatários
- ✅ Processa apenas destinatários ativos (IsActive = true)

### 3. Atualização de Status
- ✅ Reseta status para Pending antes de processar
- ✅ Atualiza status individual para Sent/Failed
- ✅ Atualiza SentAt quando status = Sent

### 4. LastSentAt
- ✅ Atualiza apenas se ao menos 1 e-mail foi enviado com sucesso
- ✅ Não atualiza se todos os envios falharam

### 5. Prioridade
- ✅ Adiciona prefixo "[URGENTE]" quando Priority = High
- ✅ Não adiciona prefixo quando Priority = Normal

## Exemplos de Testes

### Mock de Envio de E-mail

```csharp
_emailServiceMock
    .Setup(e => e.SendEmailAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>()))
    .ReturnsAsync(true);
```

### InMemory Database

```csharp
var context = TestDbContextFactory.CreateInMemoryContext("TestDb");
var repository = new ReminderRepository(context);
```

### Verify de Chamadas

```csharp
_repositoryMock.Verify(
    r => r.UpdateReminderLastSentAtAsync(
        It.IsAny<int>(),
        It.IsAny<DateTime>()),
    Times.Once);
```

## Próximos Testes (Expansão Futura)

- [ ] EmailService (mock SMTP)
- [ ] Worker integration tests
- [ ] Testes de concorrência
- [ ] Testes de performance
