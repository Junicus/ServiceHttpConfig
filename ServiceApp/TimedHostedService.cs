using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace ServiceApp
{

    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IModel _channel;

        public TimedHostedService(ILogger logger, IModel channel)
        {
            _logger = logger;
            _channel = channel;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Timed Background Service is starting.");

            _channel.ExchangeDeclare("ServiceApp.Exchange", "topic", true, false, null);
            _channel.QueueDeclare("ServiceApp.Exchange.Queue", true, false, false,  null);
            _channel.QueueBind("ServiceApp.Exchange.Queue", "ServiceApp.Exchange", "#", null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) => 
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                _logger.Information("Routing key: {0}; message: {1}", ea.RoutingKey, message);
            };

            _channel.BasicConsume("ServiceApp.Exchange.Queue", true, "", false, false, null, consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Timed Background Service is stopping.");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}