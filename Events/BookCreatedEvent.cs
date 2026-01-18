namespace LibraryCoreApi.Events;

public class BookCreatedEvent
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public string? Publisher { get; set; }
    public int TotalCopies { get; set; }
}
