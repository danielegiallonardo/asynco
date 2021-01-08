using Asynco.Samples.ServiceBus.Shared;
using Asynco.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Asynco.Samples.ServiceBus.Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services
                    .AddRemoting(options =>
                        options.UseServiceBus(options =>
                        {
                            options.Timeout = TimeSpan.FromMinutes(1);
                            options.RequestsQueueName = "<asyncrequestsqueuename>";
                            options.RepliesQueueName = "<asyncrepliesqueuename>";
                            options.FullyQualifiedNamespace = "<AzureServiceBusFullyQualifiedNamespace>";
                        }))
                    .AddRemotedReceiverFor<IRemoteService>()
                    .AddScoped<IRemoteService, RemoteService>();
                });
    }
}
