using Asynco.Models;
using System;
using System.Threading.Tasks;

namespace Asynco.Interfaces
{
    /// <summary>
    /// This interface represents a generic remoting transport 
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public interface IRemotingTransport : IAsyncDisposable
    {
        /// <summary>
        /// Sends the request on the underlying transport and waits for the correlated reply.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<RemoteReply> SendRequestAndWaitForReply(RemoteRequest request);

        /// <summary>
        /// Starts processing requests from the remoting transport using the specified processor, 
        /// and returns the replies on the same remoting transport.
        /// </summary>
        /// <param name="reqRepProcessor">The req rep processor.</param>
        /// <returns></returns>
        Task StartProcessingRequests(Func<RemoteRequest, Task<RemoteReply>> reqRepProcessor);
    }
}
