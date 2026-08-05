-- Reference SQL for the AddRoomBookings EF Core migration.
-- Use `context.Database.Migrate()` / `docker compose up --build` to apply the migration.
CREATE TABLE [RoomBookings] (
    [RoomBookingID] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MemberID] int NOT NULL,
    [RoomID] int NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Note] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ProcessedByStaffID] int NULL,
    [ProcessedAt] datetime2 NULL,
    CONSTRAINT [FK_RoomBookings_Members_MemberID] FOREIGN KEY ([MemberID]) REFERENCES [Members] ([MemberID]) ON DELETE CASCADE,
    CONSTRAINT [FK_RoomBookings_Rooms_RoomID] FOREIGN KEY ([RoomID]) REFERENCES [Rooms] ([RoomID]),
    CONSTRAINT [FK_RoomBookings_Staffs_ProcessedByStaffID] FOREIGN KEY ([ProcessedByStaffID]) REFERENCES [Staffs] ([StaffID])
);
CREATE INDEX [IX_RoomBookings_MemberID_Status] ON [RoomBookings] ([MemberID], [Status]);
CREATE INDEX [IX_RoomBookings_ProcessedByStaffID] ON [RoomBookings] ([ProcessedByStaffID]);
CREATE INDEX [IX_RoomBookings_RoomID_StartTime_EndTime] ON [RoomBookings] ([RoomID], [StartTime], [EndTime]);
