namespace LibraryCoreApi.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "Author", "Customer" // TODO make this unique and case insensitive
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;

    // Navigation properties
    public ICollection<PartyRole> PartyRoles { get; set; } = new List<PartyRole>();
}
