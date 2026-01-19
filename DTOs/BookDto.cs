namespace LibraryCoreApi.DTOs;

/// <summary>
/// BookDto class to represent a book
/// </summary>
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// CreateBookDto class to represent a book to be created
/// </summary>
public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int NumberOfCopies { get; set; } = 1;
}

/// <summary>
/// UpdateBookDto class to represent a book to be updated
/// </summary>
public class UpdateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Publisher { get; set; } = string.Empty;
}
