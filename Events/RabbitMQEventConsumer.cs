using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LibraryCoreApi.Events;

public class RabbitMQEventConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
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

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        // Initialize connection and channel synchronously in constructor
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        ).GetAwaiter().GetResult();

        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        ).GetAwaiter().GetResult();

        // Bind queue to exchange with wildcard pattern to receive all events
        _channel.QueueBindAsync(
            queue: _queueName,
            exchange: _exchangeName,
            routingKey: "#" // # matches all routing keys
        ).GetAwaiter().GetResult();

        // Set QoS to process one message at a time for better reliability
        _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).GetAwaiter().GetResult();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                var eventData = JsonSerializer.Deserialize<JsonElement>(message); // TODO check whether there is no need to deserialize since StoreEventAsync is serializing again

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
        
        if (_channel.IsOpen)
        {
            await _channel.CloseAsync();
        }
        
        if (_connection.IsOpen)
        {
            await _connection.CloseAsync();
        }
        
        _channel.Dispose();
        _connection.Dispose();
        
        await base.StopAsync(cancellationToken);
    }
}
