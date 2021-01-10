using Asynco.Samples.RabbitMQ.Shared;
using Asynco.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Asynco.Samples.RabbitMQ.Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddRemoting(options =>
                    options.UseRabbitMQ(opt =>
                    {
                        opt.HostName = "localhost";
                        opt.RequestsQueueName = "asyncrequests";
                        opt.RepliesQueueName = "asyncreplies";
                        opt.UserName = "guest";
                        opt.Password = "guest";
                    }))
                .AddRemotedService<IRemoteService>()
                .BuildServiceProvider();

            var service = services.GetService<IRemoteService>();

            var response = service.DoWork(new Request()
            {
                Payload = "Just a test payload"
            });
        }
    }
}
