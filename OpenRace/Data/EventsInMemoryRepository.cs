using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRace.Entities;
using RaceId = System.Guid;

namespace OpenRace.Data
{
    public class EventsInMemoryRepository
    {
        private readonly ConcurrentDictionary<RaceId, ConcurrentDictionary<int, ConcurrentBag<RaceEvent>>> _eventsDict = new();

        public Task AddEvent(RaceEvent @event)
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
            return Task.CompletedTask;
        }
    }
}