using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace ServiceApp
{
    public static class MetricsRegistry
    {
        public static readonly string RabbitMqContext = "RabbitMq";

        public static CounterOptions RabbitMqMessagesReceived = new CounterOptions{Name="MessagesReceived", Context=RabbitMqContext, MeasurementUnit=Unit.Calls};
    }

    public class TimedHostedService : IHostedService, IDisposable, IAmPausable
    {
        private readonly ILogger _logger;
        private readonly IModel _channel;
        private readonly IMetricsRoot _metrics;
        private string _tag;

        public Status CurrentStatus { get; private set; }

        public TimedHostedService(ILogger logger, IModel channel, IMetricsRoot metrics)
        {
            _logger = logger;
            _channel = channel;
            _metrics = metrics;
            CurrentStatus = Status.Stopped;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Timed Background Service is starting.");

            _channel.ExchangeDeclare("ServiceApp.Exchange", "topic", true, false, null);
            _channel.QueueDeclare("ServiceApp.Exchange.Queue", true, false, false,  null);
            _channel.QueueBind("ServiceApp.Exchange.Queue", "ServiceApp.Exchange", "#", null);

            Unpause();

            return Task.CompletedTask;
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var message = Encoding.UTF8.GetString(basicDeliverEventArgs.Body);
            _logger.Information("Routing key: {0}; message: {1}", basicDeliverEventArgs.RoutingKey, message);
            _metrics.Measure.Counter.Increment(MetricsRegistry.RabbitMqMessagesReceived);
        }

        public void Pause()
        {
            _logger.Debug("Pausing");
            _channel.BasicCancel(_tag);
            CurrentStatus = Status.Paused;
        }

        public void Unpause()
        {
            _logger.Debug("Unpausing");
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ConsumerOnReceived;

            _tag = _channel.BasicConsume("ServiceApp.Exchange.Queue", true, "", false, false, null, consumer);
            CurrentStatus = Status.Started;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Timed Background Service is stopping.");

            Pause();
            CurrentStatus = Status.Stopped;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}