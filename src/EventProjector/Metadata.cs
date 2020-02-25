using System;

namespace EventProjector
{
    public class Metadata
    {
        public Guid Id { get; set; }
        public Guid CausationId { get; set; }
        public Guid CoorelationId { get; set; }
    }
}
