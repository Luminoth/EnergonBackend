CREATE TABLE [dbo].[FriendGroups]
(
    [Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [GroupName] NVARCHAR(64) NOT NULL, 
    [AccountId] INT NOT NULL, 
    [ParentGroupId] INT NULL, 
    CONSTRAINT [FK_FriendGroups_ToAccounts] FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]), 
    CONSTRAINT [FK_FriendGroups_ToFriendGroups] FOREIGN KEY ([ParentGroupId]) REFERENCES [FriendGroups]([Id])
)
