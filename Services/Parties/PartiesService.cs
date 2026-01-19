using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;
using LibraryCoreApi.Events;

namespace LibraryCoreApi.Services.Parties;

public class PartiesService : IPartiesService
{
    private readonly DataContext _context;
    private readonly IEventPublisher _eventManager;

    public PartiesService(DataContext context, IEventPublisher eventManager)
    {
        _context = context;
        _eventManager = eventManager;
    }

    public async Task<IEnumerable<PartyDto>> GetParties()
    {
        var parties = await _context.Parties
            .Select(p => new PartyDto
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                Roles = p.PartyRoles.Select(pr => pr.Role.Name).ToList(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return parties;
    }

    public async Task<PartyDto> GetParty(int id)
    {
        var party = await _context.Parties
            .Include(p => p.PartyRoles)
                .ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (party == null)
        {
            throw new KeyNotFoundException("Party not found");
        }

        var partyDto = new PartyDto
        {
            Id = party.Id,
            Name = party.Name,
            Email = party.Email,
            Phone = party.Phone,
            Address = party.Address,
            Roles = party.PartyRoles.Select(pr => pr.Role.Name).ToList(),
            CreatedAt = party.CreatedAt
        };

        return partyDto;
    }

    public async Task<PartyDto> CreateParty(CreatePartyDto createDto)
    {
        // Validate roles exist
        var roles = await _context.Roles
            .Where(r => createDto.RoleIds.Contains(r.Id))
            .ToListAsync();

        if (roles.Count != createDto.RoleIds.Count)
        {
            throw new ApiException("One or more roles not found");
        }

        var party = new Party
        {
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Address = createDto.Address,
            CreatedAt = DateTime.UtcNow
        };

        _context.Parties.Add(party);
        await _context.SaveChangesAsync();

        // Add party roles
        foreach (var roleId in createDto.RoleIds)
        {
            _context.PartyRoles.Add(new PartyRole
            {
                PartyId = party.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        

        // Reload with roles
        await _context.Entry(party)
            .Collection(p => p.PartyRoles)
            .Query()
            .Include(pr => pr.Role)
            .LoadAsync();

        // Publish party created event
        var partyEvent = new PartyEvent
        {
            PartyId = party.Id,
            Name = party.Name,
            Email = party.Email,
            Phone = party.Phone,
            Address = party.Address,
            Roles = party.PartyRoles.Select(pr => pr.Role.Name).ToList(),
        };

        await _eventManager.PublishEvent("party.created", partyEvent);

        var partyDto = new PartyDto
        {
            Id = party.Id,
            Name = party.Name,
            Email = party.Email,
            Phone = party.Phone,
            Address = party.Address,
            Roles = party.PartyRoles.Select(pr => pr.Role.Name).ToList(),
            CreatedAt = party.CreatedAt
        };

        return partyDto;
    }

    public async Task<PartyDto> UpdateParty(int id, UpdatePartyDto updateDto)
    {
        var party = await _context.Parties
            .Include(p => p.PartyRoles)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (party == null)
        {
            throw new KeyNotFoundException("Party not found");
        }

        // Validate roles exist
        var roles = await _context.Roles
            .Where(r => updateDto.RoleIds.Contains(r.Id))
            .ToListAsync();

        if (roles.Count != updateDto.RoleIds.Count)
        {
            throw new KeyNotFoundException("One or more roles not found");
        }

        party.Name = updateDto.Name;
        party.Email = updateDto.Email;
        party.Phone = updateDto.Phone;
        party.Address = updateDto.Address;
        party.UpdatedAt = DateTime.UtcNow;

        // Update roles
        var existingRoleIds = party.PartyRoles.Select(pr => pr.RoleId).ToList();
        var rolesToAdd = updateDto.RoleIds.Except(existingRoleIds).ToList();
        var rolesToRemove = existingRoleIds.Except(updateDto.RoleIds).ToList();

        // Remove old roles
        var partyRolesToRemove = party.PartyRoles
            .Where(pr => rolesToRemove.Contains(pr.RoleId))
            .ToList();
        _context.PartyRoles.RemoveRange(partyRolesToRemove);

        // Add new roles
        foreach (var roleId in rolesToAdd)
        {
            _context.PartyRoles.Add(new PartyRole
            {
                PartyId = party.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // Reload with roles
        await _context.Entry(party)
            .Collection(p => p.PartyRoles)
            .Query()
            .Include(pr => pr.Role)
            .LoadAsync();

        // Publish party updated event
        var partyUpdatedEvent = new PartyEvent
        {
            PartyId = party.Id,
            Name = party.Name,
            Email = party.Email,
            Phone = party.Phone,
            Address = party.Address,
            Roles = party.PartyRoles.Select(pr => pr.Role.Name).ToList(),
        };

        await _eventManager.PublishEvent("party.updated", partyUpdatedEvent);

        return new PartyDto
        {
            Id = party.Id,
            Name = party.Name,
            Email = party.Email,
            Phone = party.Phone,
            Address = party.Address,
            Roles = party.PartyRoles.Select(pr => pr.Role.Name).ToList(),
            CreatedAt = party.CreatedAt
        };
    }

    public async Task DeleteParty(int id)
    {
        var party = await _context.Parties.FindAsync(id);
        if (party == null)
        {
            throw new KeyNotFoundException("Party not found");
        }

        _context.Parties.Remove(party);
        await _context.SaveChangesAsync();

        // Publish party deleted event
        var partyDeletedEvent = new PartyEvent
        {
            PartyId = party.Id,
            Name = party.Name,
            Email = party.Email,
            Phone = party.Phone,
            Address = party.Address,
        };

        await _eventManager.PublishEvent("party.deleted", partyDeletedEvent);
    }
}
