using EventProjector.EventStore;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventProjector.Demo
{
    class Program
    {
        public static object GetHandlers(Type t)
        {
            return new FakeEventHandler();
        }
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton<IEventHandler<FakeEvent>, FakeEventHandler>()
            .AddSingleton<IEventStream, EventStoreEventStreamer>()
            .AddSingleton(EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500"))
            .AddSingleton(serviceProvider => new EventProjector(
                serviceProvider.GetRequiredService<IEventStream>(),
                type => GetEventHandler(serviceProvider, type),
                () => GetEventHandlers(serviceProvider)))
            .BuildServiceProvider();

            var projector = serviceProvider.GetRequiredService<EventProjector>();

            await projector.Start(CancellationToken.None);
        }

        private static IEventHandler GetEventHandler(IServiceProvider sp, Type type)
        {
            try
            {
                return (IEventHandler)sp.GetRequiredService(type);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"could not resolve IEventHandler implementation for event type {type}", e);
            }
        }

        private static IEnumerable<IEventHandler> GetEventHandlers(IServiceProvider sp)
        {
            try
            {
                return sp.GetServices<IEventHandler>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"could not resolve IEventHandlers", e);
            }
        }
    }
}
