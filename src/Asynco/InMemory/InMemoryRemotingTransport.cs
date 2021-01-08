using Asynco.Interfaces;
using Asynco.Models;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Asynco.InMemory
{
    /// <summary>
    /// This class is an InMemory implementation of the remoting transport used for testing purposes
    /// It uses TPL Dataflow BufferBlocks 
    /// </summary>
    /// <seealso cref="Asynco.Interfaces.IRemotingTransport" />
    internal sealed class InMemoryRemotingTransport : IRemotingTransport
    {
        private static BufferBlock<RemoteRequest> requestsBlock = new BufferBlock<RemoteRequest>();
        private static BufferBlock<RemoteReply> repliesBlock = new BufferBlock<RemoteReply>();

        public InMemoryRemotingTransport()
        {
        }

        public ValueTask DisposeAsync()
        {
            requestsBlock.Complete();
            requestsBlock = new BufferBlock<RemoteRequest>();
            repliesBlock.Complete();
            repliesBlock = new BufferBlock<RemoteReply>();
            return default;
        }

        public async Task<RemoteReply> SendRequestAndWaitForReply(RemoteRequest request)
        {
            try
            {
                await requestsBlock.SendAsync(request);
                return await repliesBlock.ReceiveAsync();
            }
            catch (Exception)
            {
            }
            return null;
        }

        public async Task StartProcessingRequests(Func<RemoteRequest, Task<RemoteReply>> reqRepProcessor)
        {
            try
            {
                while (true)
                {
                    var request = await requestsBlock.ReceiveAsync();
                    var reply = await reqRepProcessor(request);
                    await repliesBlock.SendAsync(reply);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
