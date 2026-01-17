using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Services.Roles;

public class RolesService : IRolesService
{
    private readonly DataContext _context;

    public RolesService(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetRoles()
    {
        var roles = await _context.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return roles;
    }

    public async Task<RoleDto> GetRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);

        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        return roleDto;
    }

    public async Task<RoleDto> CreateRole(CreateRoleDto createDto)
    {
        var role = new Role
        {
            Name = createDto.Name,
            Description = createDto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        return roleDto;
    }

    public async Task<RoleDto> UpdateRole(int id, UpdateRoleDto updateDto)
    {
        var role = await _context.Roles.FindAsync(id);

        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        role.Name = updateDto.Name;
        role.Description = updateDto.Description;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };
    }

    public async Task DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }
}
