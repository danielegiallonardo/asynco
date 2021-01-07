using Asynco.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Asynco.InMemory
{
    /// <summary>
    /// 
    /// </summary>
    public static class InMemoryRemotingOptionsBuilderExtensions
    {
        /// <summary>
        /// Registers the in memory remoting transport.
        /// </summary>
        /// <param name="optionsBuilder">The options builder.</param>
        /// <returns></returns>
        public static RemotingOptionsBuilder UseInMemory(this RemotingOptionsBuilder optionsBuilder)
        {
            optionsBuilder.Services.AddSingleton<IRemotingTransport, InMemoryRemotingTransport>();
            return optionsBuilder;
        }
    }
}
