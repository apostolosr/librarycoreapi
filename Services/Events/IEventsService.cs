using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Events;

public interface IEventsService
{
    Task<EventLastIndexDto> GetBookEvents(int lastIndex = 0, int pageSize = 100);
    Task<EventLastIndexDto> GetUserEvents(int lastIndex = 0, int pageSize = 100);
}
