using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventProjector.EventStore
{
    public class EventStoreEventStreamer : IEventStream
    {
        private readonly IEventStoreConnection _connection;
        private Dictionary<string, Type> _cachedTypes;
        private Position _position;

        public EventStoreEventStreamer(IEventStoreConnection connection)
        {
            _connection = connection;
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

            if (_cachedTypes.ContainsKey(@event.Event.EventType) == false) return null;

            return new EventWrapper(
                JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Event.Data), _cachedTypes[@event.Event.EventType]),
                JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(@event.Event.Metadata)));
        }

        public async Task Start(IEnumerable<Type> types)
        {
            _cachedTypes = types.ToDictionary(type => type.FullName, type => type);
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
}
