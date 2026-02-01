using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LibraryCoreApi.Events;

/// <summary>
/// RabbitMQEventConsumer background service to consume events from RabbitMQ
/// </summary>
public class RabbitMQEventConsumer : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ConnectionFactory _factory;
    private readonly IEventStore _eventStore;
    private readonly ILogger<RabbitMQEventConsumer> _logger;
    private readonly string _exchangeName;
    private readonly string _queueName;

    public RabbitMQEventConsumer(
        IConfiguration configuration,
        IEventStore eventStore,
        ILogger<RabbitMQEventConsumer> logger)
    {
        _eventStore = eventStore;
        _logger = logger;

        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var userName = configuration["RabbitMQ:UserName"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "library_events";
        _queueName = configuration["RabbitMQ:QueueName"] ?? "event_store_queue";

        _factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await _factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(null, cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: _exchangeName,
            routingKey: "#"
        );

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel not initialized. StartAsync must complete before ExecuteAsync.");
        }

        _logger.LogInformation("RabbitMQ Event Consumer started. Listening for events on queue: {QueueName}", _queueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var body = ea.Body.ToArray();
            var eventName = ea.BasicProperties.Type ?? routingKey;

            try
            {
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received event: {EventName} with routing key: {RoutingKey}", eventName, routingKey);

                // Parse the event data
                var eventData = JsonSerializer.Deserialize<JsonElement>(message);

                // Store to MongoDB
                await _eventStore.StoreEventAsync(eventName, routingKey, eventData);

                // Acknowledge the message
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                
                _logger.LogInformation("Successfully processed and stored event: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventName} with routing key {RoutingKey}", eventName, routingKey);
                
                // Reject and requeue the message for retry
                // Set requeue to true to retry, or false to send to dead letter queue
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false, // Manual acknowledgment for reliability
            consumer: consumer
        );

        // Keep the service running until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Event Consumer is stopping...");

        if (_channel?.IsOpen == true)
        {
            await _channel.CloseAsync();
        }

        if (_connection?.IsOpen == true)
        {
            await _connection.CloseAsync();
        }

        _channel?.Dispose();
        _connection?.Dispose();

        await base.StopAsync(cancellationToken);
    }
}
