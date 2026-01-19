namespace LibraryCoreApi.Events;

/// <summary>
/// EventCleaner background service to clean events from the event store
/// </summary>
public class EventCleaner : BackgroundService
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventCleaner> _logger;

    public EventCleaner(
        IEventStore eventStore,
        ILogger<EventCleaner> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Cleaner started. Cleaning events older than 1 year");

        var cleanupInterval = TimeSpan.FromHours(24);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _eventStore.DeleteEventsOlderByTimespanAsync(TimeSpan.FromDays(365));
                _logger.LogInformation("Successfully cleaned events older than 1 year");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning events older than 1 year");
            }
            
            // Wait for the next cleanup interval
            await Task.Delay(cleanupInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event Cleaner is stopping...");
        
        await base.StopAsync(cancellationToken);
    }
}
