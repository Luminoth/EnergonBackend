CREATE TABLE [dbo].[Accounts]
(
    [Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [IsActive] BIT NOT NULL DEFAULT 0, 
    [AccountName] NVARCHAR(256) NOT NULL, 
    [EmailAddress] NVARCHAR(256) NOT NULL, 
    [UserName] NVARCHAR(256) NOT NULL, 
    [PasswordMD5] NCHAR(32) NOT NULL, 
    [PasswordSHA512] NCHAR(128) NOT NULL, 
    [EndPoint] NVARCHAR(32) NULL, 
    [SessionId] NVARCHAR(256) NULL, 
    [Visibility] INT NOT NULL DEFAULT 0, 
    [Status] NVARCHAR(1024) NULL, 
    CONSTRAINT [AK_Accounts_AccountName] UNIQUE ([AccountName])
)
