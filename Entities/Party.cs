namespace LibraryCoreApi.Entities;

public class Party
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<PartyRole> PartyRoles { get; set; } = new List<PartyRole>();
    public ICollection<Book> AuthoredBooks { get; set; } = new List<Book>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
