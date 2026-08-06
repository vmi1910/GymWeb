namespace GymWeb.ViewModels
{
    public class RoomAvailabilityViewModel
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int Floor { get; set; }
        public int Capacity { get; set; }
        public string RoomStatus { get; set; } = string.Empty;
        public bool IsFreeNow { get; set; }
        public bool IsUnderMaintenance { get; set; }
        public string? MaintenanceReason { get; set; }
        public string? CurrentActivity { get; set; }
        public List<RoomBusyBlock> BusyBlocks { get; set; } = new();

        public int CurrentOccupancy { get; set; }
        public List<CheckedInPerson> CheckedInPeople { get; set; } = new();
        public List<UpcomingMaintenanceItem> UpcomingMaintenance { get; set; } = new();
    }

    public class RoomBusyBlock
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsMaintenance { get; set; }
    }

    public class CheckedInPerson
    {
        public int CheckInID { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
    }

    public class UpcomingMaintenanceItem
    {
        public int RoomMaintenanceID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Reason { get; set; }
    }
}
