namespace LibraryCoreApi.DTOs;

/// <summary>
/// ReservationDto class to represent a reservation
/// </summary>
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

/// <summary>
/// CreateReservationDto class to represent a reservation to be created
/// </summary>
public class CreateReservationDto
{
    public int BookId { get; set; }
    public int CustomerId { get; set; }
}

/// <summary>
/// BorrowBookDto class to represent a book to be borrowed
/// </summary>
public class BorrowBookDto
{
    public int ReservationId { get; set; }
    public DateTime DueDate { get; set; }
}

/// <summary>
/// ReturnBookDto class to represent a book to be returned
/// </summary>
public class ReturnBookDto
{
    public int ReservationId { get; set; }
}

/// <summary>
/// BorrowingVisibilityDto class to represent the borrowing visibility of a book
/// </summary>
public class BorrowingVisibilityDto
{
    public string BookTitle { get; set; } = string.Empty;
    public int BookId { get; set; }
    public List<CurrentBorrowerDto> CurrentBorrowers { get; set; } = new();
}

/// <summary>
/// CurrentBorrowerDto class to represent a current borrower of a book
/// </summary>
public class CurrentBorrowerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CopyNumber { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime? DueDate { get; set; }
}
