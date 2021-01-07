using Asynco;
using Asynco.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Asynco.Tests
{
    public class InMemoryUnitTests
    {
        private static readonly ServiceProvider senderServices = new ServiceCollection()
                .AddRemoting(options =>
                    options.UseInMemory())
                .AddRemotedService<IRemoteService>()
                .BuildServiceProvider();
        private static readonly ServiceProvider receiverServices = new ServiceCollection()
                .AddRemoting(options =>
                    options.UseInMemory())
                .AddRemotedReceiverFor<IRemoteService>()
                .AddScoped<IRemoteService, RemoteService>()
                .BuildServiceProvider();

        [Fact]
        public async Task Test1()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            remoteService.DoWork();
            await hostedService.StopAsync(token);
        }

        [Fact]
        public async Task Test2()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            await remoteService.DoWorkAsync();
            await hostedService.StopAsync(token);
        }

        [Fact]
        public async Task Test3()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            var response = remoteService.DoWork(new Request()
            {
                Payload = "Test payload"
            });
            Assert.NotNull(response);
            await hostedService.StopAsync(token);
        }

        [Fact]
        public async Task Test4()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            var response = await remoteService.DoWorkAsync(new Request()
            {
                Payload = "Test payload"
            });
            Assert.NotNull(response);
            await hostedService.StopAsync(token);
        }

        [Fact]
        public async Task Test5()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            var response = await remoteService.DoWorkAsync<Response>(new Request()
            {
                Payload = "Test payload"
            });
            Assert.NotNull(response);
            await hostedService.StopAsync(token);
        }

        [Fact]
        public async Task Test6()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            var response = await remoteService.DoWorkAsync<Request>(new Request()
            {
                Payload = "Test payload"
            });
            Assert.NotNull(response);
            await hostedService.StopAsync(token);
        }

        [Fact]
        public async Task Test7()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            var response = remoteService.DoWork<Response>(new Request()
            {
                Payload = "Test payload"
            });
            Assert.NotNull(response);
            await hostedService.StopAsync(token);
        }
        
        [Fact]
        public async Task Test8()
        {
            var remoteService = senderServices.GetService<IRemoteService>();
            var hostedService = receiverServices.GetService<IHostedService>();
            var token = new System.Threading.CancellationToken();
            _ = hostedService.StartAsync(token);
            var response = remoteService.DoWork<Request>(new Request()
            {
                Payload = "Test payload"
            });
            Assert.NotNull(response);
            await hostedService.StopAsync(token);
        }
    }
}
