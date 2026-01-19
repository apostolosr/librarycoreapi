using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Events;

public interface IEventsService
{
    Task<IEnumerable<EventDto>> GetBookEvents();
    Task<IEnumerable<EventDto>> GetUserEvents();
}
