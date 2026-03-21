

namespace MyERP.Services.Sales.Events.Producers.Publishers.MassTransit;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : class;
}