using Asynco.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Asynco
{
    public static class RemotingExtensions
    {
        /// <summary>
        /// Adds the remoting framework using the specified transport
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static IServiceCollection AddRemoting(this IServiceCollection services, Action<RemotingOptionsBuilder> optionsAction)
        {
            optionsAction.Invoke(new RemotingOptionsBuilder(services));
            return services;
        }

        /// <summary>
        /// Registers a remoted service. This should be used on the Sender side instead of the normal service registration
        /// in order for the remoting proxy to be injected 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddRemotedService<T>(this IServiceCollection services)
            where T : class
        {
            services.AddScoped<T>(services =>
            {
                var proxy = DispatchProxy.Create<T, RemotingProxy>();
                (proxy as RemotingProxy).SetRemotingTransport(services.GetService<IRemotingTransport>());
                return proxy;
            });
            return services;
        }

        private static bool remotingReceiverRegistered = false;
        /// <summary>
        /// Adds a remoted receiver for the specified service. This should be used on the Receiver side 
        /// and should be followed by the normal service registration. This method registers a HostedService
        /// in order for the request messages to be processed from the specified transport.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddRemotedReceiverFor<T>(this IServiceCollection services)
        {
            if (!remotingReceiverRegistered)
            {
                services.AddHostedService<RemotingReceiver>();
                remotingReceiverRegistered = true;
            }

            RemotingReceiver.AddRegisteredService<T>();

            return services;
        }
    }
}
