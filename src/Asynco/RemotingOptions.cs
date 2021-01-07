using System;

namespace Asynco
{
    public class RemotingOptions
    {
        /// <summary>
        /// Gets or sets the remoting timeout. After the timeout expires, an exception is thrown.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; set; }
    }
}
