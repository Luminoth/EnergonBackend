/*
Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.        
 Use SQLCMD syntax to include a file in the post-deployment script.            
 Example:      :r .\myfile.sql                                
 Use SQLCMD syntax to reference a variable in the post-deployment script.        
 Example:      :setvar TableName MyTable                            
               SELECT * FROM [$(TableName)]                    
--------------------------------------------------------------------------------------
*/

-- Developer Accounts

INSERT INTO Accounts(IsActive, AccountName, EmailAddress, UserName, PasswordMD5, PasswordSHA512, Visibility)
VALUES(1, 'shane_lillie', 'shane_lillie@energonsoftware.org', 'Luminoth82',
    'f928f831e1e7400add25a4a0589ec8b0',
    'f5393a11f9659d4f72a15a27b21587d86530954c7c53394105baebc8111210f07049da26368f56a351d1c06cb94b3ec3a34242e21b330ede72500eb1132a4543',
    0);

INSERT INTO FriendGroups(AccountId, GroupName) VALUES(1, 'Test Group');

-- Test Accounts

INSERT INTO ACCOUNTS(IsActive, AccountName, EmailAddress, UserName, PasswordMD5, PasswordSHA512, Visibility)
VALUES(1, 'test_account1', 'test_account1@energonsoftware.org', 'Test User 1',
    'b93085feea6557d86376ee502f640c04',
    '7b291411a03c486a62eed1273ee1c65b0fbf5102596ac5cef0150a82fc3dc6d5af6db49b6d852d4a1adfeb8f37fc57e9f92ced1f40eb7d37b6d1bcb5bf131fe1',
    0);

INSERT INTO ACCOUNTS(IsActive, AccountName, EmailAddress, UserName, PasswordMD5, PasswordSHA512, Visibility)
VALUES(1, 'test_account2', 'test_account2@energonsoftware.org', 'Test User 2',
    'e24012b1cd74e8d1c969bbd0cdc8c6a4',
    '1609bbdd101185e6d331b4511ee0f8c729d0ec6658c37aa64c5e3b8b0762caae495548b08713cd2449b69c99fb9c300117f345916def45548db8aa762bc09d7b',
    0);

-- Test Friends

INSERT INTO AccountFriends(AccountId, FriendAccountId) VALUES(1, 2);
INSERT INTO AccountFriends(AccountId, FriendAccountId, GroupId) VALUES(1, 3, 1);