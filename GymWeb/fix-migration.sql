CREATE TABLE [__EFMigrationsHistory] 
( [MigrationId]  nvarchar(150) NOT NULL, 
[ProductVersion] nvarchar(32) NOT NULL, 
CONSTRAINT [PK___EFMigrationsHistory] 
PRIMARY KEY ([MigrationId]) ); 
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
 VALUES ('20260628095647_InitialCreate', '6.0.0'), ('20260628120322_AddMemberAndStaff', '6.0.0');