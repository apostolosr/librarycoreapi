namespace LibraryCoreApi.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public int BookCopyId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string CopyNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime? BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateReservationDto
{
    public int BookId { get; set; }
    public int CustomerId { get; set; }
}

public class BorrowBookDto
{
    public int ReservationId { get; set; }
    public DateTime DueDate { get; set; }
}

public class ReturnBookDto
{
    public int ReservationId { get; set; }
}

public class BorrowingVisibilityDto
{
    public string BookTitle { get; set; } = string.Empty;
    public int BookId { get; set; }
    public List<CurrentBorrowerDto> CurrentBorrowers { get; set; } = new();
}

public class CurrentBorrowerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CopyNumber { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime? DueDate { get; set; }
}
