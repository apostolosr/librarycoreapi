using LibraryCoreApi.Events;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Events;

public class EventsService : IEventsService
{
    private readonly IEventStore _eventStore;

    public EventsService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<EventLastIndexDto> GetBookEvents(int lastIndex = 0, int pageSize = 100)
    {
        var events = await _eventStore.GetBookEventsAsync(lastIndex, pageSize);
        var eventDtos = events.Select(e => new EventDto 
        {
            EventName = e.EventName,
            EventData = e.EventData.ToString(),
            Timestamp = e.Timestamp,
            ProcessedAt = e.ProcessedAt
        }).ToList();

        return new EventLastIndexDto
        {
            LastIndex = lastIndex + eventDtos.Count,
            Events = eventDtos
        };
    }

    public async Task<EventLastIndexDto> GetUserEvents(int lastIndex = 0, int pageSize = 100)
    {
        var events = await _eventStore.GetUserEventsAsync(lastIndex, pageSize);
         // TODO: use static adapter class to map event documents to DTOs
        var eventDtos = events.Select(e => new EventDto 
        {
            EventName = e.EventName,
            EventData = e.EventData.ToString(),
            Timestamp = e.Timestamp,
            ProcessedAt = e.ProcessedAt
        }).ToList();

        return new EventLastIndexDto
        {
            LastIndex = lastIndex + eventDtos.Count,
            Events = eventDtos
        };
    }
}
