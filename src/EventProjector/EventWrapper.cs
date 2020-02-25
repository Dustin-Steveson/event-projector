namespace EventProjector
{
    public class EventWrapper
    {
        public EventWrapper(object @event, Metadata metadata)
        {
            Event = @event;
            Metadata = metadata;
        }
        public object Event { get; }
        public Metadata Metadata { get; }

    }

    public class EventWrapper<TEvent> : EventWrapper
    {
        public EventWrapper(TEvent @event, Metadata metadata) : base(@event, metadata) { }
        public new TEvent Event { get { return (TEvent)base.Event; } }  
    }
}