namespace GymWeb.Models
{
    public class RoomBooking
    {
        public int RoomBookingID { get; set; }
        public int MemberID { get; set; }
        public virtual Member? Member { get; set; }
        public int RoomID { get; set; }
        public virtual Room? Room { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? ProcessedByStaffID { get; set; }
        public virtual Staff? ProcessedByStaff { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
