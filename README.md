# Asynco
Asynco is a remoting framework for .Net5.x, whose aim is to create a sync wrapper over an async integration, by implementing a simple request/reply pattern over an heterogeneous set of remoting transports.

Asynco was first created in order to seamlessly decouple various components and dependencies of a monolithic application, by splitting them into separate microservices and reducing the needed refactoring effort.

Given the following (synchronous) scenario:

![Before Asynco](https://github.com/danielegiallonardo/asynco/blob/main/images/Asynco_Before.png "Before Asynco")

Asynco allows you to split your components into separate microservices, by replacing your dependency's implementation with a remoting transport.

![After Asynco](https://github.com/danielegiallonardo/asynco/blob/main/images/Asynco_After.png "After Asynco")

Asynco offers the following benefits:
- It allows you to plan a progressive migration towards a microservices architecture without requiring you to put in the first place an invasive refactoring of your business logic code and architecture
- It frees you from the burden of implementing an event-driven or rest integration between different components (who wants to manage clients, connections, models, etc...)
- It reuses your existing interfaces and models (they just have to be serializable)
- It frees you from thinking of authentication and authorization concerns, since the remote calls can be considered "trusted" (no JWT tokens, no identityserver, no identity delegation, etc.)

This purpose is achieved by leveraging the typical Dependency Injection mechanisms of a .Net Core application, and allowing your code to remain quite untouched, from something like this:

```csharp
class ComponentA : IComponentA
{
	private readonly IComponentB _componentB;

	public ComponentA(IComponentB componentB)
	{
		_componentB = componentB;
	}

	public async Task DoWork()
	{
		await _componentB.DoWork();
	}
}
```

to something like this:

```csharp
class ComponentA : IComponentA
{
	private readonly IComponentB _componentB;

	public ComponentA(IComponentB componentB)
	{
		_componentB = componentB;
	}

	public async Task DoWork()
	{
		await _componentB.DoWork();
	}
}
```

They seems quite the same, don't they? Well, they are....I'm not joking, you don't have to touch your Business Logic code at all!!!

## Getting started

All you should do, in order to start using Asynco, is just install the Asynco NuGet package, separate your dependencies into different microservices, share your dependency interfaces, and do some tweaking in your `Startup.cs`, from something like this (in your monolithic application):

```csharp
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddScoped<IComponentA, ComponentA>();
		services.AddScoped<IComponentB, ComponentB>();
		...
	}
}
```

to something like this (in Microservice A):

```csharp
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddScoped<IComponentA, ComponentA>();
		.AddRemoting(options =>
			options.UseServiceBus(options =>
			{
				options.Timeout = TimeSpan.FromMinutes(1);
				options.RequestsQueueName = "<asyncrequestsqueuename>";
				options.RepliesQueueName = "<asyncrepliesqueuename>";
				options.FullyQualifiedNamespace = "<AzureServiceBusFullyQualifiedNamespace>";
			}))
			.AddRemotedService<IComponentB>()
		...
	}
}
```

and something like this (in Microservice B):

```csharp
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddScoped<IComponentB, ComponentB>();
		services.AddRemoting(options =>
			options.UseServiceBus(options =>
			{
				options.Timeout = TimeSpan.FromMinutes(1);
				options.RequestsQueueName = "<asyncrequestsqueuename>";
				options.RepliesQueueName = "<asyncrepliesqueuename>";
				options.FullyQualifiedNamespace = "<AzureServiceBusFullyQualifiedNamespace>";
			}))
			.AddRemotedReceiverFor<IComponentB>();
		...
	}
}
```

...and that's it, your ready to go!!!

## Remoting transports

Currently only 2 remoting transports are supported, `InMemoryTransport` (for testing purposes) and `ServiceBusTransport` (that's the Azure ServiceBus, btw), but if you want to contribute to expand the set of supported remoting transports (or open issues) you're really welcome.

## Installing via NuGet
`Install-Package Polly`

