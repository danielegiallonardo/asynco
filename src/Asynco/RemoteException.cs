using System;

namespace Asynco
{
    /// <summary>
    /// This exception is thrown on the Sender side when an exception is thrown on the Receiver side
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class RemoteException : Exception
    {
        /// <summary>
        /// Gets or sets the type of the original exception.
        /// </summary>
        /// <value>
        /// The type of the original exception.
        /// </value>
        public string OriginalExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the original stack trace.
        /// </summary>
        /// <value>
        /// The original stack trace.
        /// </value>
        public string OriginalStackTrace { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RemoteException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="originalExceptionType">Type of the original exception.</param>
        /// <param name="originalStackTrace">The original stack trace.</param>
        public RemoteException(string message, string originalExceptionType, string originalStackTrace) : base(message)
        {
            OriginalExceptionType = originalExceptionType;
            OriginalStackTrace = originalStackTrace;
        }

    }
}
