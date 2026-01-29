# ReminderService - Worker Service .NET 8

Worker Service otimizado para envio automático de lembretes por e-mail usando SQLite, EF Core e MailKit.

## Características

- Worker Service .NET 8 otimizado
- Verificação automática a cada 6 horas (360 minutos)
- Envio de e-mails HTML formatados via Gmail SMTP
- Persistência em SQLite com EF Core
- Logs estruturados com Serilog (rotação diária, 30 dias de retenção)
- Relacionamento N:N entre Lembretes e Destinatários
- Status individual de envio (Pending, Sent, Failed)
- Timezone: America/Sao_Paulo (E. South America Standard Time)

## Requisitos

- .NET 8 SDK
- Conta Gmail com senha de app (App Password)
- SQLite (incluído no .NET)

## Estrutura do Projeto

```
ReminderService.Worker/
├── Data/
│   └── AppDbContext.cs           # Contexto EF Core
├── Entities/
│   ├── Recipient.cs              # Destinatário
│   ├── Reminder.cs               # Lembrete
│   └── ReminderRecipient.cs      # Relacionamento N:N
├── Enums/
│   ├── ReminderStatus.cs         # Pending, Sent, Failed
│   └── ReminderPriority.cs       # Normal, High
├── Repositories/
│   ├── IReminderRepository.cs
│   └── ReminderRepository.cs     # Queries e atualizações
├── Services/
│   ├── ITemplateService.cs
│   ├── TemplateService.cs        # Renderização HTML
│   ├── IEmailService.cs
│   └── EmailService.cs           # Envio via MailKit
├── Templates/
│   └── email-template.html       # Template HTML
├── Worker.cs                     # Lógica principal
├── Program.cs                    # Configuração DI
└── appsettings.json              # Configurações
```

## Configuração

### 1. Restaurar pacotes

```bash
cd ReminderService.Worker
dotnet restore
```

### 2. Configurar credenciais Gmail

Edite `appsettings.json`:

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "seu-email@gmail.com",
  "SenderName": "Reminder Service",
  "Username": "seu-email@gmail.com",
  "Password": "sua-senha-app"
}
```

**Importante**: Use uma senha de app do Gmail, não sua senha real.

**Como gerar senha de app:**
1. Acesse https://myaccount.google.com/security
2. Ative verificação em 2 etapas
3. Vá em "Senhas de app"
4. Gere uma senha para "Aplicativo de e-mail"

### 3. Criar banco de dados e carregar dados

```bash
# Executar o Worker Service (cria banco e carrega seed automaticamente)
cd ReminderService.Worker
dotnet run
```

O serviço irá:
- Criar o banco de dados SQLite (`reminders.db`)
- Aplicar migrations automaticamente
- Carregar dados de exemplo (4 destinatários + 1 lembrete)
- Começar a verificar lembretes a cada 6 horas

**Nota**: Os dados de seed são carregados automaticamente apenas na primeira execução. O arquivo `seed.sql` está disponível para referência ou carga manual se necessário.

## Executar

```bash
dotnet run
```

O serviço irá:
1. Verificar/criar banco de dados
2. Aguardar 6 horas (ou executar imediatamente se houver lembretes pendentes)
3. Buscar lembretes que atendem aos critérios
4. Enviar e-mails para destinatários ativos
5. Atualizar status e LastSentAt
6. Repetir o ciclo

## Publicação Otimizada

```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained
```

Executável otimizado em: `bin/Release/net8.0/[runtime]/publish/`

**Tamanho esperado**: ~60-80 MB (self-contained)
**Memória**: 30-50 MB em execução
**CPU**: Quase zero (acorda a cada 6h)

## Regras de Negócio

### Quando um lembrete é enviado?

Um lembrete é processado quando:
1. `DueDate >= Data Atual`
2. `LastSentAt` é NULL (primeira vez) **OU**
3. `(Data Atual - LastSentAt) >= IntervalDays`

### Ciclo de Processamento

1. Buscar lembretes pendentes
2. Para cada lembrete:
   - Resetar status de todos ReminderRecipients para Pending
   - Renderizar template HTML
   - Enviar e-mail para cada destinatário ativo
   - Atualizar status individual (Sent/Failed)
   - Se ao menos 1 e-mail foi enviado com sucesso, atualizar LastSentAt
3. Aguardar 360 minutos
4. Repetir

### Status

- **Pending**: Aguardando envio
- **Sent**: Enviado com sucesso
- **Failed**: Falha no envio

### Prioridade

- **Normal**: E-mail com assunto padrão
- **High**: E-mail com prefixo "[URGENTE]"

## Dados de Exemplo

O arquivo `seed.sql` cria:
- 4 destinatários ativos (Julio, Ivan, Eliene, Bianca)
- 1 lembrete (Renovação Porto Seguro)
  - Vencimento: 65 dias a partir de hoje
  - Intervalo: 65 dias
  - Prioridade: Normal

## Logs

Logs são gravados em:
- Console (tempo real)
- Arquivo: `Logs/log-YYYYMMDD.txt`
- Retenção: 30 dias

## Estrutura do Banco de Dados

### Recipients
- Id (PK)
- Name
- Email
- IsActive

### Reminders
- Id (PK)
- Title
- Description
- DueDate
- IntervalDays
- LastSentAt
- Priority

### ReminderRecipients (N:N)
- Id (PK)
- ReminderId (FK)
- RecipientId (FK)
- Status (Pending/Sent/Failed)
- SentAt

## Consultas Úteis

```sql
-- Ver todos os lembretes
SELECT * FROM Reminders;

-- Ver destinatários ativos
SELECT * FROM Recipients WHERE IsActive = 1;

-- Ver status de envio
SELECT
    r.Title,
    rec.Name,
    rr.Status,
    rr.SentAt
FROM ReminderRecipients rr
JOIN Reminders r ON rr.ReminderId = r.Id
JOIN Recipients rec ON rr.RecipientId = rec.Id;

-- Resetar LastSentAt para testar novamente
UPDATE Reminders SET LastSentAt = NULL WHERE Id = 1;
```

## Troubleshooting

### E-mails não são enviados

1. Verifique credenciais Gmail em `appsettings.json`
2. Confirme que está usando senha de app (não senha normal)
3. Verifique logs em `Logs/log-*.txt`

### Lembrete não é processado

1. Verifique se `DueDate >= hoje`
2. Verifique se `LastSentAt` é NULL ou passou o intervalo
3. Confirme que há destinatários ativos (IsActive = 1)

### Banco de dados não é criado

1. Confirme que a ConnectionString está correta
2. Verifique permissões de escrita no diretório
3. Veja logs para erros de migração

## Customização

### Alterar intervalo de verificação

Edite `appsettings.json`:

```json
"WorkerSettings": {
  "IntervalInMinutes": "60"  // Verificar a cada 1 hora
}
```

### Alterar template de e-mail

Edite `Templates/email-template.html` e use os placeholders:
- `{{RecipientName}}`
- `{{Title}}`
- `{{Description}}`
- `{{DueDate}}`

### Adicionar novos destinatários

```sql
INSERT INTO Recipients (Name, Email, IsActive) VALUES
('Novo Destinatário', 'novo@example.com', 1);
```

### Adicionar novos lembretes

```sql
INSERT INTO Reminders (Title, Description, DueDate, IntervalDays, LastSentAt, Priority)
VALUES ('Título', 'Descrição', date('now', '+30 days'), 30, NULL, 0);

-- Associar destinatários
INSERT INTO ReminderRecipients (ReminderId, RecipientId, Status, SentAt)
SELECT 2, Id, 0, NULL FROM Recipients WHERE IsActive = 1;
```

## Licença

MIT
