using Asynco.Interfaces;
using Asynco.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Asynco
{
    /// <summary>
    /// This class implements a hosted service used to receive the requests from the remoting transport
    /// and process them using reflection to find the target type and method and invoke it
    /// The target type is retrieved from the registered services
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
    public class RemotingReceiver : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRemotingTransport _remotingTransport;

        private static Dictionary<string, Type> _registeredServices = new Dictionary<string, Type>();

        public RemotingReceiver(IServiceProvider serviceProvider, IRemotingTransport remotingTransport)
        {
            _serviceProvider = serviceProvider;
            _remotingTransport = remotingTransport;
        }

        /// <summary>
        /// Adds a registered service this receiver should handle.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddRegisteredService<T>()
        {
            if (!_registeredServices.ContainsKey(typeof(T).AssemblyQualifiedName))
            {
                _registeredServices.Add(typeof(T).AssemblyQualifiedName, typeof(T));
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _remotingTransport.StartProcessingRequests(
                async (requestMessage) =>
                {
                    // Search the target type in the collection of the registered services
                    if (_registeredServices.ContainsKey(requestMessage.TargetType))
                    {
                        var reply = new RemoteReply();
                        var targetType = _registeredServices[requestMessage.TargetType];

                        // Create a DI scope in order for the Scoped services (and their dependencies)
                        // to be correctly retrieved from the DI container
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            // Get the target service from the DI container
                            var service = scope.ServiceProvider.GetService(targetType);
                            if (service is not null)
                            {
                                // Search the target method, among those with the same name and generic arguments number
                                var methods = targetType.GetMethods()
                                    .Where(m => m.Name == requestMessage.TargetMethod
                                        && m.GetGenericArguments().Length == (requestMessage.GenericTypes?.Length ?? 0))
                                    .ToList();
                                List<object> parameters = new List<object>();
                                MethodInfo method = default;
                                // Search the correct method among synonyms
                                foreach (var item in methods)
                                {
                                    var currentMethod = item;
                                    if (currentMethod.IsGenericMethod)
                                    {
                                        currentMethod = currentMethod.MakeGenericMethod(requestMessage.GenericTypes.Select(t => Type.GetType(t)).ToArray());
                                    }
                                    var methodParameters = currentMethod.GetParameters();
                                    // Check the current method matches the requested ones according to its parameter types and number
                                    if (methodParameters.Length == (requestMessage.JsonParameters?.Length ?? 0))
                                    {
                                        for (int i = 0; i < (requestMessage.JsonParameters?.Length ?? 0); i++)
                                        {
                                            if (methodParameters[i].ParameterType.IsGenericParameter || methodParameters[i].ParameterType == Type.GetType(requestMessage.ParameterTypes[i]))
                                            {
                                                var parameter = JsonSerializer.Deserialize(requestMessage.JsonParameters[i], Type.GetType(requestMessage.ParameterTypes[i]));
                                                if (parameter is null)
                                                    break;
                                                parameters.Add(parameter);
                                            }
                                        }
                                        // If a match is found, use the current method and stop searching further
                                        if (parameters.Count == methodParameters.Length)
                                        {
                                            method = currentMethod;
                                            break;
                                        }
                                    }
                                }
                                // If the target method is found, invoke it and return the result in the Reply message
                                if (method is not null)
                                {
                                    try
                                    {
                                        var methodResult = method.Invoke(service, parameters.Count > 0 ? parameters.ToArray() : null);
                                        if (methodResult != null)
                                        {
                                            if (methodResult is Task task)
                                            {
                                                await task;

                                                if (task.GetType().IsGenericType)
                                                {
                                                    var result = methodResult.GetType().GetProperty("Result").GetValue(methodResult);
                                                    reply.ReturnType = result.GetType().AssemblyQualifiedName;
                                                    reply.JsonReturnValue = JsonSerializer.Serialize(result);
                                                }
                                            }
                                            else
                                            {
                                                reply.ReturnType = methodResult.GetType().AssemblyQualifiedName;
                                                reply.JsonReturnValue = JsonSerializer.Serialize(methodResult);
                                            }
                                        }
                                    }
                                    catch (TargetInvocationException ex)
                                    {
                                        reply.FillExceptionData(ex.InnerException);
                                    }
                                    catch (Exception ex)
                                    {
                                        reply.FillExceptionData(ex);
                                    }
                                }
                                else
                                {
                                    reply.FillExceptionData(new MissingMethodException($"The method {requestMessage.TargetMethod} is not found in the type {requestMessage.TargetType}"));
                                }
                            }
                            else
                            {
                                reply.FillExceptionData(new TypeLoadException($"A service for type {targetType.AssemblyQualifiedName} is not registered"));
                            }
                        }
                        return reply;
                    }
                    else
                    {
                        return null;
                    }
                });


        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _remotingTransport.DisposeAsync();
        }
    }
}
