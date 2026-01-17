using Microsoft.AspNetCore.Mvc;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Services.Roles;

namespace LibraryCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        var roles = await _rolesService.GetRoles();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
        var role = await _rolesService.GetRole(id);
        return Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createDto)
    {
        var role = await _rolesService.CreateRole(createDto);
        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(int id, UpdateRoleDto updateDto)
    {
        var role = await _rolesService.UpdateRole(id, updateDto);
        return Ok(role);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        await _rolesService.DeleteRole(id);
        return NoContent();
    }
}
