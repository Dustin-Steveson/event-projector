using System.Threading.Tasks;

namespace EventProjector
{
    public interface IEventStream
    {
        Task Start();

        Task Stop();

        Task<EventWrapper> GetEvent();
    }
}
