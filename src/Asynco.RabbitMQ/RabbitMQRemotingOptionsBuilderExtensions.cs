using Asynco.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Asynco.RabbitMQ
{
    public static class RabbitMQRemotingOptionsBuilderExtensions
    {
        /// <summary>
        /// Registers the RabbitMQ-based remoting transport.
        /// </summary>
        /// <param name="optionsBuilder">The options builder.</param>
        /// <param name="serviceBusOptionsAction">The service bus options action.</param>
        /// <returns></returns>
        public static RemotingOptionsBuilder UseRabbitMQ(this RemotingOptionsBuilder optionsBuilder,
            Action<RabbitMQRemotingOptions> serviceBusOptionsAction)
        {
            optionsBuilder.Services.Configure(serviceBusOptionsAction);
            optionsBuilder.Services.AddSingleton<IRemotingTransport, RabbitMQRemotingTransport>();
            return optionsBuilder;
        }
    }
}
