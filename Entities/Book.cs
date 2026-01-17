namespace LibraryCoreApi.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Party Author { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}
