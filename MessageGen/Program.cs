using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace MessageGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var instance = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                EndpointResolverFactory = (Func<IEnumerable<AmqpTcpEndpoint>, IEndpointResolver>) (endpoints => (IEndpointResolver) new DefaultEndpointResolver(new [] {new AmqpTcpEndpoint("localhost", -1)})),
                VirtualHost = "/",
                UserName = "guest",
                Password = "guest",
                RequestedHeartbeat = 60
            };
            var connection = instance.CreateConnection();
            var channel = connection.CreateModel();

            var random = new Random((int)DateTime.UtcNow.Ticks);
            var count = 0;
            var routingKey1 = "routing.key.1";
            var routingKey2 = "routing.key.2";

            channel.ExchangeDeclare("ServiceApp.Exchange", "topic", true, false, null);

            while(true)
            {
                var pause = random.Next(1, 51);
                channel.BasicPublish("ServiceApp.Exchange", pause % 2 == 0 ? routingKey1 : routingKey2, null, Encoding.UTF8.GetBytes($"This is message {++count}"));
                Thread.Sleep(pause);
            }
        }
    }
}
