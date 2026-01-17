namespace LibraryCoreApi.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "Fiction", "Mystery"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
