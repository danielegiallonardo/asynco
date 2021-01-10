namespace Asynco.RabbitMQ
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Asynco.RemotingOptions" />
    public class RabbitMQRemotingOptions : RemotingOptions
    {
        /// <summary>
        /// Gets or sets the name of the requests queue. This queue must be created with Sessions = false
        /// </summary>
        /// <value>
        /// The name of the requests queue.
        /// </value>
        public string RequestsQueueName { get; set; }
        /// <summary>
        /// Gets or sets the name of the replies queue. This queue must be created with Sessions = true
        /// </summary>
        /// <value>
        /// The name of the replies queue.
        /// </value>
        public string RepliesQueueName { get; set; }
        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public string HostName { get; set; }
        /// <summary>
        /// Gets or sets the exchange. Default is string empty
        /// </summary>
        /// <value>
        /// The exchange.
        /// </value>
        public string Exchange { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the port. Default is 5672
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port { get; set; } = 5672;
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the consumer dispatch concurrency. Default is 1
        /// </summary>
        /// <value>
        /// The consumer dispatch concurrency.
        /// </value>
        public int ConsumerDispatchConcurrency { get; set; } = 1;
    }
}
