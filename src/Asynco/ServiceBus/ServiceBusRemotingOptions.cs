using Azure.Core;
using Azure.Identity;

namespace Asynco.ServiceBus
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Asynco.RemotingOptions" />
    public class ServiceBusRemotingOptions : RemotingOptions
    {
        /// <summary>
        /// Gets or sets the connection string for the ServiceBus. 
        /// If specified, it's used first, otherwise FullyQualifiedNamespace + Credential (ManagedIdentity scenario) is used.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the fully qualified namespace for the ServiceBus. It's used with Credential in order to implement 
        /// a ManagedIdentity scenario
        /// </summary>
        /// <value>
        /// The fully qualified namespace.
        /// </value>
        public string FullyQualifiedNamespace { get; set; }
        /// <summary>
        /// Gets or sets the credential. It's used with FullyQualifiedNamespace in order to implement 
        /// a ManagedIdentity scenario
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        public TokenCredential Credential { get; set; } = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
            {
                ExcludeAzureCliCredential = false,
                ExcludeEnvironmentCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeManagedIdentityCredential = false,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true
            });
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
        /// Gets or sets the maximum concurrent calls that should be processed by the receiver. Default is 16
        /// </summary>
        /// <value>
        /// The maximum concurrent calls.
        /// </value>
        public int MaxConcurrentCalls { get; set; } = 16;
    }
}
