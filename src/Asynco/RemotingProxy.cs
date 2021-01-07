using Asynco.Interfaces;
using Asynco.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Asynco
{
    /// <summary>
    /// This class represents a transparent DispatchProxy used to intercept the calls to the service methods
    /// </summary>
    /// <seealso cref="System.Reflection.DispatchProxy" />
    public class RemotingProxy : DispatchProxy
    {
        private IRemotingTransport _remotingTransport;

        public RemotingProxy()
        {
        }

        /// <summary>
        /// Sets the remoting transport.
        /// </summary>
        /// <param name="remotingTransport">The remoting transport.</param>
        public void SetRemotingTransport(IRemotingTransport remotingTransport)
        {
            _remotingTransport = remotingTransport;
        }

        /// <summary>
        /// Whenever any method on the generated proxy type is called, this method is invoked to dispatch control.
        /// </summary>
        /// <param name="targetMethod">The method the caller invoked.</param>
        /// <param name="args">The arguments the caller passed to the method.</param>
        /// <returns>
        /// The object to return to the caller, or <see langword="null" /> for void methods.
        /// </returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            RemoteReply reply = SendRequestAndWaitForReply(targetMethod, args).ConfigureAwait(false).GetAwaiter().GetResult();
            // If the method does not return void
            if (!string.IsNullOrWhiteSpace(reply.JsonReturnValue))
            {
                // Deserialize the returned object
                var result = JsonSerializer.Deserialize(reply.JsonReturnValue, Type.GetType(reply.ReturnType));
                // If the expected return type is Task or Task<T>, just wrap the returned object with a completed Task
                if (targetMethod.ReturnType == typeof(Task) || targetMethod.ReturnType.IsSubclassOf(typeof(Task)))
                {
                    if (targetMethod.ReturnType.IsGenericType)
                    {
                        return Convert(targetMethod.ReturnType.GetGenericArguments().Last(), result);
                    }
                    return Task.CompletedTask;
                }
                return result;
            }
            return default;
        }

        private async Task<RemoteReply> SendRequestAndWaitForReply(MethodInfo method, object[] args)
        {
            // Create a remote request
            var request = new RemoteRequest()
            {
                JsonParameters = args?.Select(a => JsonSerializer.Serialize(a))?.ToArray(),
                ParameterTypes = args?.Select(a => a.GetType().AssemblyQualifiedName)?.ToArray(),
                GenericTypes = method.IsGenericMethod ? method.GetGenericArguments()?.Select(t => t.AssemblyQualifiedName).ToArray() : null,
                TargetType = method.DeclaringType.AssemblyQualifiedName,
                TargetMethod = method.Name
            };

            // Send the request and wait for the reply
            var reply = await _remotingTransport.SendRequestAndWaitForReply(request);

            // If an exception has been thrown on the receiver side, include the original data in a new exception
            if (reply.ExceptionType is not null)
            {
                var ex = new RemoteException(reply.ExceptionMessage,
                    reply.ExceptionType,
                    reply.ExceptionStackTrace);
                ExceptionDispatchInfo.Throw(ex);
            }

            return reply;
        }

        /// <summary>
        /// Wrap an object with a completed Task.
        /// </summary>
        /// <param name="genericType">Type of the generic.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private static Task Convert(Type genericType, object result)
        {
            var fromResultMethod = typeof(Task).GetMethod("FromResult", BindingFlags.Static | BindingFlags.Public);
            var fromResultGenericMethod = fromResultMethod.MakeGenericMethod(genericType);
            return fromResultGenericMethod.Invoke(null, new object[] { result }) as Task;
        }
    }
}
