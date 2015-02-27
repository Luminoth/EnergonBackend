CREATE TABLE [dbo].[StartupEvents]
(
    [Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [Timestamp] DATETIME NOT NULL DEFAULT GETDATE(), 
    [Type] INT NOT NULL, 
    [Application] NVARCHAR(32) NOT NULL
)
