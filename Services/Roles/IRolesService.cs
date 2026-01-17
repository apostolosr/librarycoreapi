using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Roles;

public interface IRolesService
{
    Task<IEnumerable<RoleDto>> GetRoles();
    Task<RoleDto> GetRole(int id);
    Task<RoleDto> CreateRole(CreateRoleDto createDto);
    Task<RoleDto> UpdateRole(int id, UpdateRoleDto updateDto);
    Task DeleteRole(int id);
}
