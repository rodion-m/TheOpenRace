using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using OpenRace.Data;
using OpenRace.Data.Ef;
using OpenRace.Entities;

namespace OpenRace.Features.RaceEvents
{
    public class RaceEventsManager
    {
        private readonly RaceEventsDbRepository _repo;
        private readonly RaceEventsCache _eventsCache;
        private readonly IQueue _queue;
        private readonly RaceEventsSubscriptionManager _subscriptionManager;

        public RaceEventsManager(
            RaceEventsDbRepository repo, 
            RaceEventsCache eventsCache, 
            IQueue queue, 
            RaceEventsSubscriptionManager subscriptionManager)
        {
            _repo = repo;
            _eventsCache = eventsCache;
            _queue = queue;
            _subscriptionManager = subscriptionManager;
        }

        public Task AddAsync(RaceEvent @event)
        {
            _eventsCache.Add(@event);
            _queue.QueueAsyncTask(() => _repo.AddAsync(@event));
            if (@event.EventType != EventType.RaceFinished)
            {
                _subscriptionManager.NotifyEventAdded(@event);
            }
            return Task.CompletedTask;
        }



        public async IAsyncEnumerable<RaceEvent> GetRaceEvents(Guid raceId, int distance)
        {
            //TODO
            // if (_eventsCache.TryGetEvents(raceId, distance, out var events))
            // {
            //     
            // }
            // else
            // {
            //      //await WaitPendingOperations();
            //     var dbEvents = await _repo.GetRaceEvents(raceId, distance).ToListAsync();
            //     _eventsCache.Fill(dbEvents);
            // }
            await WaitPendingOperations();
            await foreach (var raceEvent in _repo.GetRaceEvents(raceId, distance))
            {
                yield return raceEvent;
            }
        }

        /// <summary>
        /// In memory DB is NOT using here.
        /// </summary>
        public async Task<RaceEvent?> GetLastEventByCreatorOrNull(string creatorName)
        {
            //TODO get from cache
            await WaitPendingOperations();
            return await _repo.GetLastEventByCreatorOrNull(creatorName);
        }

        public async Task WaitPendingOperations()
        {
            var queueMetrics = _queue.GetMetrics();
            while (queueMetrics.WaitingCount() > 0 || queueMetrics.RunningCount() > 0)
            {
                await Task.Delay(50);
                queueMetrics = _queue.GetMetrics();
            }
        }

        public Task DeleteAsync(RaceEvent @event)
        {
            _eventsCache.Delete(@event);
            _queue.QueueAsyncTask(() => _repo.DeleteAsync(@event));
            _subscriptionManager.NotifyEventDeleted(@event);
            return Task.CompletedTask;
        }

        public void ClearCache(bool notifyListeners)
        {
            _eventsCache.Clear();
            if (notifyListeners)
            {
                _subscriptionManager.OnAllEventsShouldBeUpdated();
            }
        }
    }
}