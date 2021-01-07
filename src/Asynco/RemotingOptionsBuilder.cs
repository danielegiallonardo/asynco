using Microsoft.Extensions.DependencyInjection;

namespace Asynco
{
    /// <summary>
    /// A builder class for the Remoting framework
    /// </summary>
    public class RemotingOptionsBuilder
    {
        public readonly IServiceCollection Services;

        public RemotingOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
