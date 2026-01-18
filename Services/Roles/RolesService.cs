using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;
using LibraryCoreApi.Events;

namespace LibraryCoreApi.Services.Roles;

public class RolesService : IRolesService
{
    private readonly DataContext _context;
    private readonly IEventPublisher _eventManager;

    public RolesService(DataContext context, IEventPublisher eventManager)
    {
        _context = context;
        _eventManager = eventManager;
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
        // Check if role already exists
        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == createDto.Name);
        if (existingRole != null)
        {
            throw new ApiException("Role already exists");
        }

        var role = new Role
        {
            Name = createDto.Name,
            Description = createDto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Publish role created event
        var roleCreatedEvent = new RoleEvent
        {
            RoleId = role.Id,
            Name = role.Name,
            Description = role.Description
        };

        await _eventManager.PublishEvent("role.created", roleCreatedEvent);

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

        // Check if role already exists
        var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == updateDto.Name);
        if (existingRole != null && existingRole.Id != id)
        {
            throw new ApiException("Role already exists");
        }

        role.Name = updateDto.Name;
        role.Description = updateDto.Description;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish role updated event
        var roleUpdatedEvent = new RoleEvent
        {
            RoleId = role.Id,
            Name = role.Name,
            Description = role.Description,
        };

        await _eventManager.PublishEvent("role.updated", roleUpdatedEvent);

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

        // Publish role deleted event
        var roleDeletedEvent = new RoleEvent
        {
            RoleId = role.Id,
            Name = role.Name,
            Description = role.Description
        };

        await _eventManager.PublishEvent("role.deleted", roleDeletedEvent);
    }
}
