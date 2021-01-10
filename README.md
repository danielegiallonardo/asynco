# Asynco
Asynco is a remoting framework for .Net, whose aim is to create a sync wrapper over an async integration, by implementing a simple request/reply pattern over an heterogeneous set of remoting transports.

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
		services.AddRemoting(options =>
			options.UseServiceBus(options =>
			{
				options.Timeout = TimeSpan.FromMinutes(1);
				options.RequestsQueueName = "<asyncrequestsqueuename>";
				options.RepliesQueueName = "<asyncrepliesqueuename>";
				options.FullyQualifiedNamespace = "<AzureServiceBusFullyQualifiedNamespace>";
			}))
			.AddRemotedService<IComponentB>();
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

...and that's it, you're ready to go!!!

## Remoting transports

Currently 3 remoting transports are supported, `InMemoryTransport` (for testing purposes), `ServiceBusTransport` (that's the Azure ServiceBus, btw), and `RabbitMQTransport`, but if you want to contribute to expand the set of supported remoting transports (or open issues) you're really welcome.

### InMemory Transport
This transport exists only for testing purposes (it's the one that is used in the unit tests), and it makes possible to use Asynco when the Sender and the Receiver are co-located in the same process, since it uses two shared TPL Dataflow's BufferBlocks for message passing. Just set it up by using 

```csharp
services.AddRemoting(options =>
	options.UseInMemory())
```

and you're ready to go.

### ServiceBus Transport
This transport uses two Azure ServiceBus queues, one for the requests and one for the replies. The one for the requests must be set with Session=false, and on the other hand the one for the replies must be set with Session=true (since it's used to correlate a reply with its request). Just set it up by using

```csharp
services.AddRemoting(options =>
	options.UseServiceBus(opt =>
	{
		// This is used on the sender side, it's the DispatchProxy timeout 
		opt.Timeout = TimeSpan.FromMinutes(1); 
		// This queue must have the setting Session=false
		opt.RequestsQueueName = "<asyncrequestsqueuename>";
		// This queue must have the setting Session=true
		opt.RepliesQueueName = "<asyncrepliesqueuename>";
		// You could specify a ConnectionString
		opt.ConnectionString = "<azureservicebusconnectionstring>",
		// Or, alternatively, you could use these two options for the ManagedIdentity scenario
		// (the Credential setting is optional, and the shown value it's the default one,
		// you could just specify the namespace and let the library do the rest)
		opt.FullyQualifiedNamespace = "<AzureServiceBusFullyQualifiedNamespace>";
		opt.Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
		{
			ExcludeAzureCliCredential = false,
			ExcludeEnvironmentCredential = true,
			ExcludeInteractiveBrowserCredential = true,
			ExcludeManagedIdentityCredential = false,
			ExcludeSharedTokenCacheCredential = true,
			ExcludeVisualStudioCodeCredential = true,
			ExcludeVisualStudioCredential = true
		})
	}))
```

### RabbitMQ Transport
This transport uses two RabbitMQ queues, one for the requests and one for the replies. In order for the correlation between requests and replies to be achieved, it uses the direct reply-to mechanism as decribed [here](https://www.rabbitmq.com/direct-reply-to.html). Just set it up by using

```csharp
services.AddRemoting(options =>
	options.UseRabbitMQ(opt =>
	{
		// This is used on the sender side, it's the DispatchProxy timeout 
		opt.Timeout = TimeSpan.FromMinutes(1); 
		opt.RequestsQueueName = "<asyncrequestsqueuename>";
		opt.RepliesQueueName = "<asyncrepliesqueuename>";
		opt.HostName = "localhost";
		opt.UserName = "<username>";
		opt.Password = "<password>";
		// Optional, default is 5672
		opt.Port = 5672;
		// Optional, default is string empty
		opt.Exchange = "";
		// Optional, default is 1
		opt.ConsumerDispatchConcurrency = 1;
	}))
```



More on the remoting transports in the Samples.

## Installing via NuGet
`Install-Package Asynco`

