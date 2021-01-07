using Asynco.Interfaces;
using Asynco.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Asynco.ServiceBus
{
    /// <summary>
    /// This class implements the Azure ServiceBus remoting transport.
    /// In order for the correlation between requests and replies to be achieved, it uses the request/reply pattern
    /// as decribed here: https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sessions#request-response-pattern
    /// </summary>
    /// <seealso cref="Asynco.Interfaces.IRemotingTransport" />
    internal sealed class ServiceBusRemotingTransport : IRemotingTransport
    {
        private readonly ServiceBusRemotingOptions _options;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _requestSender;
        private readonly ServiceBusSender _replysender;
        private ServiceBusProcessor processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusRemotingTransport"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public ServiceBusRemotingTransport(IOptions<ServiceBusRemotingOptions> options)
        {
            _options = options.Value;
            if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                _serviceBusClient = new ServiceBusClient(_options.ConnectionString);
            }
            else
            {
                _serviceBusClient = new ServiceBusClient(_options.FullyQualifiedNamespace, _options.Credential);
            }
            _requestSender = _serviceBusClient.CreateSender(_options.RequestsQueueName);
            _replysender = _serviceBusClient.CreateSender(_options.RepliesQueueName);
        }

        public async ValueTask DisposeAsync()
        {
            if (processor is not null)
            {
                if (processor.IsProcessing)
                {
                    await processor.StopProcessingAsync();
                }
                await processor.DisposeAsync();
            }
            await _requestSender.DisposeAsync();
            await _replysender.DisposeAsync();
            await _serviceBusClient.DisposeAsync();
        }

        public async Task<RemoteReply> SendRequestAndWaitForReply(RemoteRequest request)
        {
            var sessionId = Guid.NewGuid().ToString();

            var requestMessage = new ServiceBusMessage()
            {
                ReplyToSessionId = sessionId,
                Body = BinaryData.FromObjectAsJson(request)
            };

            await _requestSender.SendMessageAsync(requestMessage);

            var sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_options.RepliesQueueName, sessionId,
                new ServiceBusSessionReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });

            var replyMessage = await sessionReceiver.ReceiveMessageAsync(_options.Timeout);
            if (replyMessage != null)
            {
                return replyMessage.Body.ToObjectFromJson<RemoteReply>();
            }
            else
            {
                throw new TimeoutException($"No reply was received in the allotted time");
            }
        }

        public async Task StartProcessingRequests(Func<RemoteRequest, Task<RemoteReply>> reqRepProcessor)
        {
            processor = _serviceBusClient.CreateProcessor(_options.RequestsQueueName, new ServiceBusProcessorOptions()
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = _options.MaxConcurrentCalls
            });

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var requestMessage = args.Message.Body.ToObjectFromJson<RemoteRequest>();

                RemoteReply reply = await reqRepProcessor.Invoke(requestMessage);
                if (reply is not null)
                {
                    var replyMessage = new ServiceBusMessage()
                    {
                        SessionId = args.Message.ReplyToSessionId,
                        Body = BinaryData.FromObjectAsJson(reply)
                    };
                    await _replysender.SendMessageAsync(replyMessage);
                    await args.CompleteMessageAsync(args.Message);
                }
                else
                {
                    await args.AbandonMessageAsync(args.Message);
                }
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                // TODO: define error handler logic
                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync();
        }
    }
}
