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
    public async Task<ActionResult<IEnumerable<EventDto>>> GetBookEvents()
    {
        var events = await _eventsService.GetBookEvents();
        return Ok(events);
    }

    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetUserEvents()
    {
        var events = await _eventsService.GetUserEvents();
        return Ok(events);
    }
}
