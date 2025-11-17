namespace OrderFlow.Application.Common.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class;
}