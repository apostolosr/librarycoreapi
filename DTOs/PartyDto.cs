namespace LibraryCoreApi.DTOs;

/// <summary>
/// PartyDto class to represent a party
/// </summary>
public class PartyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// CreatePartyDto class to represent a party to be created
/// </summary>
public class CreatePartyDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<int> RoleIds { get; set; } = [];
}

/// <summary>
/// UpdatePartyDto class to represent a party to be updated
/// </summary>
public class UpdatePartyDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<int> RoleIds { get; set; } = [];
}
