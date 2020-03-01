using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventProjector.Demo
{

    public class FakeEventStream : IEventStream
    {
        public FakeEventStream()
        {
        }

        public Task Start(IEnumerable<Type> types)
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }

        public Task<EventWrapper> GetEvent()
        {
            return Task.FromResult(new EventWrapper<FakeEvent>(new FakeEvent { Name = "Test" }, new Metadata()) as EventWrapper);
        }
    }

    public class FakeEvent
    {
        public string Name { get; set; }
    }

    public class FakeEvent2
    {
        public string Name { get; set; }
    }

    public class FakeEventHandler : IEventHandler<FakeEvent>
    {
        public async Task Handle(EventWrapper<FakeEvent> eventWrapper)
        {
            Console.WriteLine(eventWrapper.Event.Name);
            await Task.Delay(100);
        }
    }
}
