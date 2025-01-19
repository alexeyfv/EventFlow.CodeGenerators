using EventFlow.Aggregates;
using EventFlow.Core;
using Microsoft.Extensions.DependencyInjection;
using Order.AggregateStore;
using Order.Events;
using Order.Subscribers;

Console.WriteLine("Hello, World!");

var serviceProvider = new ServiceCollection()
    .BuildServiceProvider();

IAggregateStore store = serviceProvider.GetRequiredService<IAggregateStore>();

var id = OrderAggregateId.New;
var cancellationToken = CancellationToken.None;

await store.UpdateAsync(
    id,
    order => order.DoSomething(),
    cancellationToken);

public class OrderAggregateId(string value) : Identity<OrderAggregateId>(value);

public class OrderAggregate(OrderAggregateId id) : AggregateRoot<OrderAggregate, OrderAggregateId>(id)
{
    public Task DoSomething() => Task.CompletedTask;
}

public class CustomOrderEvent : OrderEvent;

public class OrderSubscribers : ISubscribeSynchronousTo<CustomOrderEvent>
{
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderAggregateId, CustomOrderEvent> domainEvent, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
