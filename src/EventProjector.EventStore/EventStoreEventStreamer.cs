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
        public async Task<IEnumerable<EventWrapper>> GetEvents()
        {
            var eventStoreResponse = await _connection.ReadAllEventsForwardAsync(_position, 100, false);

            _position = eventStoreResponse.NextPosition;

            var events = eventStoreResponse
                .Events
                .Where(e => _cachedTypes.ContainsKey(e.Event.EventType))
                .Select(e => new EventWrapper(
                JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Event.Data), _cachedTypes[e.Event.EventType]),
                JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(e.Event.Metadata))));

            return events;
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
