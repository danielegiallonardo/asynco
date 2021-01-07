namespace Asynco.Models
{
    /// <summary>
    /// This class represents the reply message exchanged on the remoting transport
    /// </summary>
    public class RemoteRequest
    {
        /// <summary>
        /// Gets or sets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        public string TargetType { get; set; }
        /// <summary>
        /// Gets or sets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public string TargetMethod { get; set; }
        /// <summary>
        /// Gets or sets the json parameters.
        /// </summary>
        /// <value>
        /// The json parameters.
        /// </value>
        public string[] JsonParameters { get; set; }
        /// <summary>
        /// Gets or sets the generic types (in case of a generic method).
        /// </summary>
        /// <value>
        /// The generic types.
        /// </value>
        public string[] GenericTypes { get; set; }
        /// <summary>
        /// Gets or sets the parameter types.
        /// </summary>
        /// <value>
        /// The parameter types.
        /// </value>
        public string[] ParameterTypes { get; set; }
    }
}
