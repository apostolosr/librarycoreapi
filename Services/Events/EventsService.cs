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

    public async Task<IEnumerable<EventDto>> GetBookEvents()
    {
        var events = await _eventStore.GetBookEventsAsync();
        return events.Select(e => new EventDto 
        {
            EventName = e.EventName,
            EventData = e.EventData.ToString(),
            Timestamp = e.Timestamp,
            ProcessedAt = e.ProcessedAt
        });
    }

    public async Task<IEnumerable<EventDto>> GetUserEvents()
    {
        var events = await _eventStore.GetUserEventsAsync();
        return events.Select(e => new EventDto 
        {
            EventName = e.EventName,
            EventData = e.EventData.ToString(),
            Timestamp = e.Timestamp,
            ProcessedAt = e.ProcessedAt
        });
    }
}
