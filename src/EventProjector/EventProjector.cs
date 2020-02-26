using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventProjector
{
    public class EventProjector
    {
        private readonly IEventStream _eventStream;
        private readonly Func<Type, object> _handlerResolver;
        private readonly IDictionary<Type, Func<EventWrapper, Task>> _processEventDelegateCache;

        public EventProjector(IEventStream eventStream, Func<Type, object> handlerResolver)
        {
            _eventStream = eventStream;
            _handlerResolver = handlerResolver;
            _processEventDelegateCache = new Dictionary<Type, Func<EventWrapper, Task>>();
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await _eventStream.Start();
            while (cancellationToken.IsCancellationRequested == false)
            {
                var eventWrapper = await _eventStream.GetEvent();
                if (eventWrapper == null) continue;
                if (_processEventDelegateCache.ContainsKey(eventWrapper.Event.GetType()) == false)
                {
                    _processEventDelegateCache.Add(eventWrapper.Event.GetType(), GetProcessEventDelegate(eventWrapper.Event.GetType()));
                }
                
                await _processEventDelegateCache[eventWrapper.Event.GetType()](eventWrapper);
            }
        }

        private Func<EventWrapper, Task> GetProcessEventDelegate(Type type)
        {
            var methodInfo = GetType()
                .GetMethod(nameof(EventProjector.ProcessEvent), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type);

            var instanceParameter = Expression.Constant(this);
            var argumentsParameter = Expression.Parameter(typeof(object), "arguments");

            var eventType = typeof(EventWrapper<>).MakeGenericType(type);

            var call = Expression.Call(
                instanceParameter,
                methodInfo,
                Expression.Convert(argumentsParameter, eventType));

            return Expression.Lambda<Func<EventWrapper, Task>>(
                    call,
                    argumentsParameter).Compile();
        }

        private async Task ProcessEvent<TEvent>(EventWrapper @event)
        {
            var handler = _handlerResolver(typeof(IEventHandler<TEvent>)) as IEventHandler<TEvent>;
            await handler.Handle(@event as EventWrapper<TEvent>);
        }
    }
}
