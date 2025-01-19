using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EventFlow.CodeGenerators;

[Generator]
public class SourceCodeGenerator : IIncrementalGenerator
{
    private const string ClassPostfix = "Aggregate";

    private const string SourceCodeTemplate =
    """
    using EventFlow.Aggregates;
    using EventFlow.Aggregates.ExecutionResults;
    using EventFlow.Core;
    using EventFlow.Subscribers;

    namespace {0}.Events
    {{
        /// <summary>
        /// A base class for all events of the <see cref="{0}Aggregate"/>
        /// </summary>
        public abstract class {0}Event :
            IAggregateEvent<{0}Aggregate, {0}AggregateId>;
    }}

    namespace {0}.Subscribers
    {{
        /// <summary>
        /// An interface for synchronous subscribers of the <see cref="{0}Aggregate"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of the event</typeparam>
        public interface ISubscribeSynchronousTo<TEvent> :
            ISubscribeSynchronousTo<{0}Aggregate, {0}AggregateId, TEvent>
            where TEvent : Events.{0}Event;

        /// <summary>
        /// An interface for asynchronous subscribers of the <see cref="{0}Aggregate"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of the event</typeparam>
        public interface ISubscribeAsynchronousTo<TEvent> :
            ISubscribeAsynchronousTo<{0}Aggregate, {0}AggregateId, TEvent>
            where TEvent : Events.{0}Event;
    }}

    namespace {0}.AggregateStore
    {{
        public static class IAggregateStoreExtensions
        {{
            /// <summary>
            /// Updates the <see cref="{0}Aggregate"/>
            /// </summary>
            /// <param name="aggregateStore">Aggregate store</param>
            /// <param name="id">ID of the aggregate</param>
            /// <param name="update">Update function</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>A task that return a collection of domain events that happened during the update</returns>
            public static Task<IReadOnlyCollection<IDomainEvent>> UpdateAsync(
                this IAggregateStore aggregateStore,
                {0}AggregateId id,
                Func<{0}Aggregate, Task> update,
                CancellationToken cancellationToken) =>
                    aggregateStore.UpdateAsync<{0}Aggregate, {0}AggregateId>(
                        id,
                        SourceId.New,
                        (aggregate, _) => update(aggregate),
                        cancellationToken);

            /// <summary>
            /// Loads the <see cref="{0}Aggregate"/>
            /// </summary>
            /// <param name="aggregateStore">Aggregate store</param>
            /// <param name="id">ID of the aggregate</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>A task that returns the <see cref="{0}Aggregate"/></returns>
            public static Task<{0}Aggregate> LoadAsync(
                this IAggregateStore aggregateStore,
                {0}AggregateId id,
                CancellationToken cancellationToken) =>
                    aggregateStore.LoadAsync<{0}Aggregate, {0}AggregateId>(
                        id,
                        cancellationToken);

            /// <summary>
            /// Updates the <see cref="{0}Aggregate"/>
            /// </summary>
            /// <typeparam name="TExecutionResult">The type of the execution result</typeparam>
            /// <param name="aggregateStore">Aggregate store</param>
            /// <param name="id">ID of the aggregate</param>
            /// <param name="update">Update function</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>A task that returns an <see cref="IAggregateUpdateResult{{TExecutionResult}}"/></returns>
            public static Task<IAggregateUpdateResult<TExecutionResult>> UpdateAsync<TExecutionResult>(
                this IAggregateStore aggregateStore,
                {0}AggregateId id,
                Func<{0}Aggregate, Task<TExecutionResult>> update,
                CancellationToken cancellationToken)
                    where TExecutionResult : IExecutionResult =>
                    aggregateStore.UpdateAsync<{0}Aggregate, {0}AggregateId, TExecutionResult>(
                        id,
                        SourceId.New,
                        (aggregate, _) => update(aggregate),
                        cancellationToken);

            /// <summary>
            /// Stores the <see cref="{0}Aggregate"/>
            /// </summary>
            /// <param name="aggregate">Aggregate to store</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>A task that return a collection of domain events that happened during the update</returns>
            public static Task<IReadOnlyCollection<IDomainEvent>> StoreAsync(
                this IAggregateStore aggregateStore,
                {0}Aggregate aggregate,
                CancellationToken cancellationToken) =>
                    aggregateStore.StoreAsync<{0}Aggregate, {0}AggregateId>(
                        aggregate,
                        SourceId.New,
                        cancellationToken);
        }}
    }}
    """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            static (syntaxNode, _) =>
            {
                // Filter out not classes
                if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax)
                {
                    return false;
                }

                // We use convention that all aggregate classes end with "Aggregate"
                if (!classDeclarationSyntax.Identifier.ValueText.EndsWith(ClassPostfix, StringComparison.Ordinal))
                {
                    return false;
                }

                return true;
            },
            static (syntaxContext, _) => (ClassDeclarationSyntax)syntaxContext.Node);

        context.RegisterSourceOutput(provider, static (ctx, classDeclarationSyntax) =>
        {
            var featureName = classDeclarationSyntax.Identifier.ValueText.Replace(ClassPostfix, string.Empty);

            var source = string.Format(SourceCodeTemplate, featureName);

            ctx.AddSource(
                $"{featureName}.g.cs", 
                SourceText.From(source, Encoding.UTF8));
        });
    }
}
