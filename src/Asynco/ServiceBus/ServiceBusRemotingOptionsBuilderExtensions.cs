using Asynco.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Asynco.ServiceBus
{
    public static class ServiceBusRemotingOptionsBuilderExtensions
    {
        /// <summary>
        /// Registers the ServiceBus-based remoting transport.
        /// </summary>
        /// <param name="optionsBuilder">The options builder.</param>
        /// <param name="serviceBusOptionsAction">The service bus options action.</param>
        /// <returns></returns>
        public static RemotingOptionsBuilder UseServiceBus(this RemotingOptionsBuilder optionsBuilder,
            Action<ServiceBusRemotingOptions> serviceBusOptionsAction)
        {
            optionsBuilder.Services.Configure(serviceBusOptionsAction);
            optionsBuilder.Services.AddSingleton<IRemotingTransport, ServiceBusRemotingTransport>();
            return optionsBuilder;
        }
    }
}
