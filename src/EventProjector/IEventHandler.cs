using System.Threading.Tasks;

namespace EventProjector
{
    public interface IEventHandler<TEvent>
    {
        Task Handle(EventWrapper<TEvent> eventWrapper);
    }
}
