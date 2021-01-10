using System;

namespace Asynco
{
    public class RemotingOptions
    {
        /// <summary>
        /// Gets or sets the remoting timeout. After the timeout expires, an exception is thrown. Default is 1 minute.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
    }
}
