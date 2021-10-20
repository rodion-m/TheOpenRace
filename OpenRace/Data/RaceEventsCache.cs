using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRace.Entities;
using RaceId = System.Guid;

namespace OpenRace.Data
{
    public class RaceEventsCache
    {
        private readonly ConcurrentDictionary<RaceId, ConcurrentDictionary<int, ConcurrentBag<RaceEvent>>> _eventsDict = new();

        public void Add(RaceEvent @event)
        {
            _eventsDict.AddOrUpdate(@event.RaceId,
                _ =>
                {
                    var raceEvents = new ConcurrentDictionary<int, ConcurrentBag<RaceEvent>>(
                        new Dictionary<int, ConcurrentBag<RaceEvent>>()
                        {
                            { @event.Distance, new ConcurrentBag<RaceEvent>() { @event } }
                        });
                    return raceEvents;
                }, (_, raceEvents) =>
                {
                    raceEvents.AddOrUpdate(@event.Distance,
                        _ => new ConcurrentBag<RaceEvent>() { @event }, (_, distanceEvents) =>
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
                    raceEvents[@event.Distance] = new ConcurrentBag<RaceEvent>(distanceEvents.Where(it => it.Id != @event.Id));
                }
            }
        }

        public void Clear() => _eventsDict.Clear();
    }
}