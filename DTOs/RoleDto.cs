namespace LibraryCoreApi.DTOs;

/// <summary>
/// RoleDto class to represent a role
/// </summary>
public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// CreateRoleDto class to represent a role to be created
/// </summary>
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// UpdateRoleDto class to represent a role to be updated
/// </summary>
public class UpdateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
