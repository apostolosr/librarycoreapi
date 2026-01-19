using Microsoft.AspNetCore.Mvc;
using LibraryCoreApi.Services.Events;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;
    public EventsController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [HttpGet("book")]
    public async Task<ActionResult<EventLastIndexDto>> GetBookEvents([FromQuery] int lastIndex = 0, [FromQuery] int pageSize = 100)
    {
        var result = await _eventsService.GetBookEvents(lastIndex, pageSize);
        return Ok(result);
    }

    [HttpGet("user")]
    public async Task<ActionResult<EventLastIndexDto>> GetUserEvents([FromQuery] int lastIndex = 0, [FromQuery] int pageSize = 100)
    {
        var result = await _eventsService.GetUserEvents(lastIndex, pageSize);
        return Ok(result);
    }
}
