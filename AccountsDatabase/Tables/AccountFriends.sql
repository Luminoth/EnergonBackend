CREATE TABLE [dbo].[AccountFriends]
(
    [Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [AccountId] INT NOT NULL, 
    [FriendAccountId] INT NOT NULL, 
    [GroupId] INT NULL, 
    CONSTRAINT [FK_AccountFriends_ToAccounts] FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]), 
    CONSTRAINT [FK_AccountFriends_Friend_ToAccounts] FOREIGN KEY ([FriendAccountId]) REFERENCES [Accounts]([Id])
)
