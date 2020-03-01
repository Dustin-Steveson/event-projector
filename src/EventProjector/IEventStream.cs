using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventProjector
{
    public interface IEventStream
    {
        Task Start(IEnumerable<Type> eventTypes);

        Task Stop();

        Task<IEnumerable<EventWrapper>> GetEvents();
    }
}
