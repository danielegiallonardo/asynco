using Asynco.Samples.ServiceBus.Shared;
using Asynco.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Asynco.Samples.ServiceBus.Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddRemoting(options =>
                    options.UseServiceBus(options =>
                    {
                        options.Timeout = TimeSpan.FromMinutes(1);
                        options.RequestsQueueName = "<asyncrequestsqueuename>";
                        options.RepliesQueueName = "<asyncrepliesqueuename>";
                        options.FullyQualifiedNamespace = "<AzureServiceBusFullyQualifiedNamespace>";
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
