using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using OpenRace.Data;
using OpenRace.Data.Ef;
using OpenRace.Entities;
using OpenRace.Pages.Referee;
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
        
        public async Task RemoveBadLaps(Guid raceId, int distance, Duration minLapDuration)
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

        public async Task AddExtraAverageLaps(Guid raceId, List<RaceResults.RaceResult> results)
        {
            foreach (var raceResult in results.Where(it => 
                !it.IsAllLapsComplete && it.FinishedAt != null && it.FinishedLaps >= it.TotalLaps / 2))
            {
                var remainingLaps = raceResult.TotalLaps - raceResult.FinishedLaps;
                var time = raceResult.FinishedAt!.Value;
                var newRaceEvents = new List<RaceEvent>();
                for (int i = 0; i < remainingLaps; i++)
                {
                    time += raceResult.AverageLap!.Value;
                    newRaceEvents.Add(new RaceEvent(Guid.NewGuid(), raceId, raceResult.Member.Number!.Value, LapComplete, time, "_CORRECTOR_", raceResult.Distance));
                }

                await _repo.AddAll(newRaceEvents);

                var finish = await _repo.GetRaceEvents(raceId, raceResult.Distance)
                    .FirstOrDefaultAsync(it => it.MemberNumber == raceResult.Member.Number && it.EventType == RaceFinished);
                if (finish != null)
                {
                    finish.TimeStamp = time;
                    await _repo.UpdateAsync(finish);
                }
                else
                {
                    finish = new RaceEvent(Guid.NewGuid(), raceId, raceResult.Member.Number!.Value, RaceFinished, time,
                        "_CORRECTOR_", raceResult.Distance);
                    await _repo.AddAsync(finish);
                }
            }
            _eventsCache.Clear();
            _subscriptionManager.OnAllEventsShouldBeUpdated();
        }
    }
}