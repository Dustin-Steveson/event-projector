using System.Threading.Tasks;

namespace EventProjector
{
    public interface IEventHandler<TEvent> : IEventHandler
    {
        Task Handle(EventWrapper<TEvent> eventWrapper);
    }

    public interface IEventHandler { }
}
