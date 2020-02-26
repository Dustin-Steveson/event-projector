using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventProjector.EventStore
{
    public class EventStoreEventStreamer : IEventStream
    {
        private readonly IEventStoreConnection _connection;
        private readonly EventAssemblies _eventAssemblies;
        private Dictionary<string, Type> _cachedTypes;
        private Position _position;

        public EventStoreEventStreamer(IEventStoreConnection connection, EventAssemblies eventAssemblies)
        {
            _connection = connection;
            _eventAssemblies = eventAssemblies;
            _position = new Position(0, 0);
            _cachedTypes = new Dictionary<string, Type>();
        }
        public async Task<EventWrapper> GetEvent()
        {
            var events = await _connection.ReadAllEventsForwardAsync(_position, 1, false, new UserCredentials("admin", "changeit"));

            if(events.Events.Length == 0)
            {
                return null;
            }

            _position = events.NextPosition;

            var @event = events.Events.SingleOrDefault();

            if(_cachedTypes.ContainsKey(@event.Event.EventType) == false)
            {
                var type = _eventAssemblies.Assemblies
                    .SelectMany(a => a.GetTypes())
                    .SingleOrDefault(t => t.Name == @event.Event.EventType);

                if(type == null)
                {
                    return null;
                }

                _cachedTypes.Add(@event.Event.EventType, type);
            }

            return new EventWrapper(
                JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Event.Data), _cachedTypes[@event.Event.EventType]),
                JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(@event.Event.Metadata)));
        }

        public async Task Start()
        {
            await _connection.ConnectAsync();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }

    public class EventStoreEventStreamerSettings
    {
        public ushort NumberOfEventsPerPull { get; set; }
    }

    public class EventAssemblies
    {
        public EventAssemblies(params Assembly[] assemblies)
        {
            Assemblies = assemblies;
        }

        public IEnumerable<Assembly> Assemblies { get; }
    }
}
