using System;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using OpenRace.Data;
using OpenRace.Data.Ef;
using static OpenRace.Entities.EventType;

namespace OpenRace.Features.RaceEvents
{
    public class RaceEventsFixer
    {
        public RaceEventsFixer(
            RaceEventsDbRepository repo,
            RaceEventsCache eventsCache,
            RaceEventsSubscriptionManager subscriptionManager)
        {
            _repo = repo;
            _eventsCache = eventsCache;
            _subscriptionManager = subscriptionManager;
        }
        
        private readonly RaceEventsDbRepository _repo;
        private readonly RaceEventsCache _eventsCache;
        private readonly RaceEventsSubscriptionManager _subscriptionManager;
        
        private async Task RemoveBadLaps(Guid raceId, int distance, Duration minLapDuration)
        {
            var events = await _repo.GetRaceEvents(raceId, distance)
                .ToLookupAsync(it => it.MemberNumber);
            foreach (var memberEvents in events)
            {
                var raceEvents = memberEvents.Where(it => it.EventType != RaceFinished)
                    .OrderBy(it => it.TimeStamp)
                    .ToArray();
                for (var i = 1; i < raceEvents.Length; i++)
                {
                    var prevEvent = raceEvents[i - 1];
                    var e = raceEvents[i];
                    var interval = e.TimeStamp - prevEvent.TimeStamp;
                    if (interval < minLapDuration)
                    {
                        await _repo.DeleteAsync(e);
                    }
                }
            }

            _eventsCache.Clear();
            _subscriptionManager.OnAllEventsShouldBeUpdated();
        }

        public async Task AddExtraAverageLaps(Guid raceId)
        {
            _repo.GetAllRaceEvents(raceId);
            _eventsCache.Clear();
            _subscriptionManager.OnAllEventsShouldBeUpdated();
        }
    }
}