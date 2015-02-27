CREATE TABLE [dbo].[AuthenticateEvents]
(
    [Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [Timestamp] DATETIME NOT NULL DEFAULT GETDATE(), 
    [Type] INT NOT NULL, 
    [Origin] NVARCHAR(32) NOT NULL, 
    [AccountName] NVARCHAR(256) NULL, 
    [Reason] NVARCHAR(256) NULL
)
