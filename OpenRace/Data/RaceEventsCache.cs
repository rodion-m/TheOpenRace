using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OpenRace.Entities;
using RaceId = System.Guid;

namespace OpenRace.Data
{
    public class RaceEventsCache
    {
        private class VersionedEvents
        {
            private long _version;

            public VersionedEvents(ConcurrentDictionary<System.Guid, RaceEvent> events)
            {
                Events = events;
            }

            public VersionedEvents(RaceEvent firstEvent)
            {
                Events = new ConcurrentDictionary<RaceId, RaceEvent>(new Dictionary<RaceId, RaceEvent>()
                {
                    { firstEvent.Id, firstEvent }
                });
            }

            public ConcurrentDictionary<System.Guid, RaceEvent> Events { get; }

            public long GetVersion() => Interlocked.Read(ref _version);

            public long Add(RaceEvent @event)
            {
                Interlocked.Increment(ref _version);
                Events.TryAdd(@event.Id, @event);
                return _version;
            }

            public long Delete(System.Guid eventId)
            {
                Interlocked.Increment(ref _version);
                Events.TryRemove(eventId, out _);
                return _version;
            }
        }

        private readonly ConcurrentDictionary<RaceId, ConcurrentDictionary<int, VersionedEvents>> _eventsDict = new();

        public void Add(RaceEvent @event)
        {
            _eventsDict.AddOrUpdate(@event.RaceId,
                _ =>
                {
                    var raceEvents = new ConcurrentDictionary<int, VersionedEvents>(
                        new Dictionary<int, VersionedEvents>()
                        {
                            { @event.Distance, new VersionedEvents(@event) }
                        });
                    return raceEvents;
                }, (_, raceEvents) =>
                {
                    raceEvents.AddOrUpdate(@event.Distance,
                        _ => new VersionedEvents(@event), (_, distanceEvents) =>
                        {
                            distanceEvents.Add(@event);
                            return distanceEvents;
                        }
                    );
                    return raceEvents;
                });
        }

        public void Delete(RaceEvent @event)
        {
            if (_eventsDict.TryGetValue(@event.RaceId, out var raceEvents))
            {
                if (raceEvents.TryGetValue(@event.Distance, out var distanceEvents))
                {
                    distanceEvents.Delete(@event.Id);
                }
            }
        }

        public void Clear() => _eventsDict.Clear();
    }
}