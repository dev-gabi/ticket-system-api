DELETE FROM tickets
DBCC CHECKIDENT ('ticket_system.dbo.tickets', RESEED, 0)