namespace MyERP.SalesServiceTutorial.Events.Publishers;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : BaseEvent;
}