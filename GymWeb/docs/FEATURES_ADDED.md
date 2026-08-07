# Feature Implementation Log

## 2026-08-04 — Room booking and work schedules

### New files

- `Models/RoomBooking.cs`: booking entity with member, room, time range, status, notes, and processing audit fields.
- `ViewModels/RoomBookingCreateViewModel.cs`: validated member booking input.
- `Services/BookingConflictService.cs`: prevents approval when an approved booking overlaps in the same room.
- `Services/ScheduleConflictService.cs`: prevents staff or room shifts from overlapping.
- `Controllers/RoomBookingController.cs`: member request/history/cancel flow and staff/admin approval flow.
- `Controllers/MyScheduleController.cs`: personal schedule view for Staff and Trainer.
- `Views/RoomBooking/*` and `Views/MySchedule/Index.cshtml`: Razor/Bootstrap UI reusing the existing layouts.
- `Migrations/20260804000000_AddRoomBookings.cs`: adds the `RoomBookings` table and indexes.
- `docs/sql/20260804000000_AddRoomBookings.sql`: reference SQL corresponding to the migration.

### Minimal edits to existing files

- `Models/DataContext.cs`: registers `RoomBookings`, its foreign keys, and indexes.
- `Program.cs`: registers the two conflict-checking services.
- `Controllers/ScheduleController.cs`: calls the new schedule conflict service when creating or editing a shift.
- `Controllers/AccountController.cs`: sends Trainer accounts to their personal schedule after login.
- `Views/Shared/_Layout.cshtml` and `_AdminLayout.cshtml`: adds navigation for the new screens.

### Database behavior

The existing startup call to `context.Database.Migrate()` applies the new table automatically when a user pulls this source, configures SQL Server, and starts the application. Existing migrations and tables are unchanged.

### Verification completed

- `docker compose up --build -d` completed successfully.
- Migration `20260804000000_AddRoomBookings` created the table and all three indexes in the Docker SQL Server database.
- HTTP smoke test: a Member registered, created a booking, and an Admin could view and approve it.
- Conflict tests: a second booking overlapping an approved room booking was rejected; an overlapping staff/room shift was rejected.

## 2026-08-05 — Staff/Trainer accounts and self-service shift registration

### New files

- `ViewModels/ScheduleRegistrationViewModel.cs`: validated self-service shift registration input.
- `Views/MySchedule/Register.cshtml`: Staff/Trainer form for registering a work shift.

### Existing-file integrations

- `Controllers/MyScheduleController.cs`: identifies the signed-in Staff/Trainer, registers a shift, and uses `ScheduleConflictService` to reject conflicts with any existing staff or room schedule.
- `Controllers/AdminController.cs`: accepts an optional preset role for account-creation links.
- `Views/Staff/Index.cshtml` and `Views/Trainer/Index.cshtml`: add role-specific **Tạo tài khoản** buttons that open the existing account form with Staff or Trainer preselected.
- `Views/MySchedule/Index.cshtml`: adds the **Đăng ký ca** entry point.

No database schema change is required: Staff/Trainer accounts use the existing `Accounts` and `Staffs` tables; self-registered shifts use `StaffSchedules`.

### Verification completed

- Docker build and restart completed successfully.
- Admin smoke test: the preset Staff account page rendered, a Staff account was created, and the new Staff could log in and see the personal schedule page.
- Self-service smoke test: the Staff registered a future shift and it appeared in the personal schedule.
- Conflict test: a Trainer could not add a second overlapping shift; SQL Server contained only the original schedule row.

## 2026-08-05 — Trainer sidebar authorization

- `Views/Shared/_AdminLayout.cshtml`: limits member management, payments, and subscriptions to Admin/Staff. Trainer now sees only the personal schedule/shift-registration flow and the return-home/logout actions.

## User documentation

- `docs/HUONG_DAN_SU_DUNG_TINH_NANG_MOI.md`: Vietnamese, interface-based guide for Staff/Trainer accounts, self-service shifts, room booking, approvals, roles, and conflict rules.
