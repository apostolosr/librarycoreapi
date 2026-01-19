namespace LibraryCoreApi.DTOs;

public class EventDto
{
    public string EventName { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

public class EventLastIndexDto
{
    public int LastIndex { get; set; }
    public IEnumerable<EventDto> Events { get; set; } = new List<EventDto>();
}