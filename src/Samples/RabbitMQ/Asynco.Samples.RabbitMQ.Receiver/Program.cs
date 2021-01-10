using Asynco.Samples.RabbitMQ.Shared;
using Asynco.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Asynco.Samples.RabbitMQ.Receiver
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
                        options.UseRabbitMQ(opt =>
                        {
                            opt.HostName = "localhost";
                            opt.RequestsQueueName = "asyncrequests";
                            opt.RepliesQueueName = "asyncreplies";
                            opt.UserName = "guest";
                            opt.Password = "guest";
                        }))
                    .AddRemotedReceiverFor<IRemoteService>()
                    .AddScoped<IRemoteService, RemoteService>();
                });
    }
}
