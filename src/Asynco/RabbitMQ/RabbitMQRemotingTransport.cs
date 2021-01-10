using Asynco.Interfaces;
using Asynco.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Asynco.RabbitMQ
{
    /// <summary>r
    /// This class implements the RabbitMQ remoting transport.
    /// In order for the correlation between requests and replies to be achieved, it uses the direct reply-to mechanism
    /// as decribed here: https://www.rabbitmq.com/direct-reply-to.html
    /// </summary>
    /// <seealso cref="Asynco.Interfaces.IRemotingTransport" />
    internal sealed class RabbitMQRemotingTransport : IRemotingTransport
    {
        private readonly RabbitMQRemotingOptions _options;
        private readonly IConnection connection;
        private IModel receiverChannel;
        private EventingBasicConsumer receiverConsumer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQRemotingTransport" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public RabbitMQRemotingTransport(IOptions<RabbitMQRemotingOptions> options)
        {
            _options = options.Value;
            var factory = new ConnectionFactory()
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                ConsumerDispatchConcurrency = _options.ConsumerDispatchConcurrency
            };
            connection = factory.CreateConnection();
        }

        public ValueTask DisposeAsync()
        {
            connection.Dispose();
            return default;
        }

        public async Task<RemoteReply> SendRequestAndWaitForReply(RemoteRequest request)
        {
            var sessionId = Guid.NewGuid().ToString();

            using var channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);
            var props = channel.CreateBasicProperties();
            props.CorrelationId = sessionId;
            props.ReplyTo = _options.RepliesQueueName;

            BufferBlock<RemoteReply> respQueue = new BufferBlock<RemoteReply>();

            consumer.Received += async (model, ea) =>
            {
                var response = JsonSerializer.Deserialize<RemoteReply>(ea.Body.ToArray());
                if (ea.BasicProperties.CorrelationId == sessionId)
                {
                    await respQueue.SendAsync(response);
                }
            };

            channel.BasicPublish(
                exchange: _options.Exchange,
                routingKey: _options.RequestsQueueName,
                basicProperties: props,
                body: JsonSerializer.SerializeToUtf8Bytes(request));

            channel.BasicConsume(
                consumer: consumer,
                queue: _options.RepliesQueueName,
                autoAck: true);

            RemoteReply result = default;
            try
            {
                result = await respQueue.ReceiveAsync(_options.Timeout);
            }
            catch (Exception)
            {
            }
            return result ?? throw new TimeoutException($"No reply was received in the allotted time");
        }

        public Task StartProcessingRequests(Func<RemoteRequest, Task<RemoteReply>> reqRepProcessor)
        {
            receiverChannel = connection.CreateModel();
            receiverConsumer = new EventingBasicConsumer(receiverChannel);

            receiverConsumer.Received += async (model, ea) =>
            {
                var requestMessage = JsonSerializer.Deserialize<RemoteRequest>(ea.Body.ToArray());
                var props = ea.BasicProperties;
                var replyProps = receiverChannel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                RemoteReply reply = await reqRepProcessor.Invoke(requestMessage);
                if (reply is not null)
                {
                    receiverChannel.BasicPublish(exchange: _options.Exchange,
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: JsonSerializer.SerializeToUtf8Bytes(reply));
                    receiverChannel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }
                else
                {
                    receiverChannel.BasicNack(deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }

            };
            receiverChannel.BasicConsume(queue: _options.RequestsQueueName,
                autoAck: false,
                consumer: receiverConsumer);
            return Task.CompletedTask;
        }
    }
}
