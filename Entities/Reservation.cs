namespace LibraryCoreApi.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int BookCopyId { get; set; }
    public int CustomerId { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Reserved;

    // Navigation properties
    public BookCopy BookCopy { get; set; } = null!;
    public Party Customer { get; set; } = null!;
}

public enum ReservationStatus
{
    Reserved = 0,
    Borrowed = 1,
    Returned = 2,
    Cancelled = 3
}
