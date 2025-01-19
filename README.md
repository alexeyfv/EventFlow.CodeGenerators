# EventFlow.CodeGenerators

A source generator library designed to minimize boilerplate code when working with the [EventFlow](https://github.com/eventflow/EventFlow) library.

## Features

- **Event Class Generation**: Automatically generate base event classes for aggregates.
- **Subscriber Interfaces**: Generate synchronous and asynchronous subscriber interfaces for events.
- **Aggregate Store Extensions**: Simplify aggregate update, load, and store operations with generated extension methods.

## Installation

1. Add the library to your project by referencing the NuGet package:

    ``` bash
    dotnet add package EventFlow.CodeGenerators
    ```

1. Ensure your project is configured to support source generators (available in .NET 5.0 and above).

## Usage

This library relies on naming conventions to identify aggregates. Your aggregate classes should end with the word `Aggregate` (e.g., `OrderAggregate`, `CustomerAggregate`).

Let’s consider the following aggregate:

``` csharp
public class OrderAggregateId(string value) :
    Identity<OrderAggregateId>(value);

public class OrderAggregate(OrderAggregateId id) : 
    AggregateRoot<OrderAggregate, OrderAggregateId>(id)
{
    public Task DoSomething() 
    {
        // Some code
    }
}
```

Using the `EventFlow.CodeGenerators` library, the following code will be automatically generated:

### Base Class for Events

 A base class for the events related to the OrderAggregate:

 ``` csharp
 namespace Order.Events;

 public abstract class OrderEvent :
     IAggregateEvent<OrderAggregate, OrderAggregateId>;
 ```

### Subscriber Interfaces

Interfaces for subscribing to events synchronously and asynchronously:

``` csharp
namespace Order.Subscribers;

public interface ISubscribeSynchronousTo<TEvent> : 
    ISubscribeSynchronousTo<OrderAggregate, OrderAggregateId, TEvent>
    where TEvent : Events.OrderEvent;

public interface ISubscribeAsynchronousTo<TEvent> :
    ISubscribeAsynchronousTo<OrderAggregate, OrderAggregateId, TEvent>
    where TEvent : Events.OrderEvent;
```

Instead of writing this:

``` csharp
public class OrderSubscribers : 
    ISubscribeSynchronousTo<OrderAggregate, OrderAggregateId, CustomOrderEvent>
{
    public Task HandleAsync(
        IDomainEvent<OrderAggregate, OrderAggregateId, CustomOrderEvent> domainEvent, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
```

You can now write this simplified version:

``` csharp
public class OrderSubscribers : 
    ISubscribeSynchronousTo<CustomOrderEvent>
{
    public Task HandleAsync(
        IDomainEvent<OrderAggregate, OrderAggregateId, CustomOrderEvent> domainEvent, 
        CancellationToken cancellationToken)
    {
        // Some code
    }
}
```

### Extension Methods for `IAggregateStore`

Extension methods that simplify interactions with the `IAggregateStore`, allowing you to omit type declarations. For example:

Instead of writing this:

``` csharp
await store.UpdateAsync<OrderAggregate, OrderAggregateId>(
    id,
    SourceId.New,
    (order, _) => order.DoSomething(),
    cancellationToken);
```

You can now write this simplified version:

``` csharp
await store.UpdateAsync(
    id,
    order => order.DoSomething(),
    cancellationToken);
```

## Contributing

Contributions are welcome! If you find issues or want to suggest improvements, feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
