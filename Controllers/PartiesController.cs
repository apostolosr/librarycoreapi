using Microsoft.AspNetCore.Mvc;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Services.Parties;

namespace LibraryCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartiesController : ControllerBase
{
    private readonly IPartiesService _partiesService;

    public PartiesController(IPartiesService partiesService)
    {
        _partiesService = partiesService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartyDto>>> GetParties()
    {
        var parties = await _partiesService.GetParties();
        return Ok(parties);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PartyDto>> GetParty(int id)
    {
        var party = await _partiesService.GetParty(id);
        return Ok(party);
    }

    [HttpPost]
    public async Task<ActionResult<PartyDto>> CreateParty(CreatePartyDto createDto)
    {
        var party = await _partiesService.CreateParty(createDto);
        return CreatedAtAction(nameof(GetParty), new { id = party.Id }, party);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PartyDto>> UpdateParty(int id, UpdatePartyDto updateDto)
    {
        var party = await _partiesService.UpdateParty(id, updateDto);
        return Ok(party);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteParty(int id)
    {
        await _partiesService.DeleteParty(id);
        return NoContent();
    }
}
