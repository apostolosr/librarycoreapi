namespace LibraryCoreApi.Entities;

public class BookCopy
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string CopyNumber { get; set; } = string.Empty; // Unique identifier for this copy
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Book Book { get; set; } = null!;
    public Reservation? CurrentReservation { get; set; }
}
