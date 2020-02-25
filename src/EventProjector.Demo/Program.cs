using Microsoft.Extensions.DependencyInjection;
using System;
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
            .AddSingleton<IEventStream, FakeEventStream>()
            .AddSingleton(serviceProvider => new EventProjector(serviceProvider.GetRequiredService<IEventStream>(), type => GetEventHandler(serviceProvider, type)))
            .BuildServiceProvider();

            var projector = serviceProvider.GetRequiredService<EventProjector>();

            await projector.Start(CancellationToken.None);
        }

        private static object GetEventHandler(IServiceProvider sp, Type type)
        {
            try
            {
                return sp.GetRequiredService(type);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"could not resolve IHandleEvents implementation for event type {type}", e);
            }
        }
    }
}
