namespace LibraryCoreApi.Entities;

public class PartyRole
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Party Party { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
