-- Inserir 4 destinatários
INSERT INTO Recipients (Name, Email, IsActive) VALUES
('Julio', 'juli-microlins@hotmail.com', 1),
('Ivan', 'ivanpicanha@hotmail.com', 1),
('Eliene', 'elienerochasantana@gmail.com', 1),
('Bianca', 'bia.maced@hotmail.com', 1);

-- Inserir lembrete de exemplo (Porto Seguro - renovação a cada 65 dias)
INSERT INTO Reminders (Title, Description, DueDate, IntervalDays, LastSentAt, Priority) VALUES
('Renovação Porto Seguro',
 'Lembrete para renovação do seguro Porto Seguro. Verificar condições, coberturas e realizar o pagamento dentro do prazo.',
 date('now', '+65 days'),
 65,
 NULL,
 0);

-- Associar todos os destinatários ao lembrete
INSERT INTO ReminderRecipients (ReminderId, RecipientId, Status, SentAt)
SELECT 1, Id, 0, NULL FROM Recipients;
