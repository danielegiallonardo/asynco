using System;

namespace Asynco.Models
{
    /// <summary>
    /// This class represents the reply message exchanged on the remoting transport
    /// </summary>
    public class RemoteReply
    {
        /// <summary>
        /// Gets or sets the type of the returned object.
        /// </summary>
        /// <value>
        /// The type of the return.
        /// </value>
        public string ReturnType { get; set; }
        /// <summary>
        /// Gets or sets the json return value.
        /// </summary>
        /// <value>
        /// The json return value.
        /// </value>
        public string JsonReturnValue { get; set; }
        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        /// <value>
        /// The exception message.
        /// </value>
        public string ExceptionMessage { get; set; }
        /// <summary>
        /// Gets or sets the type of the exception.
        /// </summary>
        /// <value>
        /// The type of the exception.
        /// </value>
        public string ExceptionType { get; set; }
        /// <summary>
        /// Gets or sets the exception stack trace.
        /// </summary>
        /// <value>
        /// The exception stack trace.
        /// </value>
        public string ExceptionStackTrace { get; set; }

        /// <summary>
        /// Fills the exception data in the current object.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void FillExceptionData(Exception ex)
        {
            ExceptionType = ex.GetType().AssemblyQualifiedName;
            ExceptionMessage = ex.Message;
            ExceptionStackTrace = ex.StackTrace;
        }
    }
}
