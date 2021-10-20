using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Queuing;
using Coravel.Queuing.Interfaces;
using OpenRace.Data;
using OpenRace.Data.Ef;
using OpenRace.Entities;

namespace OpenRace.Features.Events
{
    public class EventsManager
    {
        private readonly EventsDbRepository _repo;
        private readonly EventsInMemoryRepository _inMemoryRepo;
        private readonly IQueue _queue;
        private readonly EventsSubscriptionManager _subscriptionManager;

        public EventsManager(
            EventsDbRepository repo, 
            EventsInMemoryRepository inMemoryRepo, 
            IQueue queue, 
            EventsSubscriptionManager subscriptionManager)
        {
            _repo = repo;
            _inMemoryRepo = inMemoryRepo;
            _queue = queue;
            _subscriptionManager = subscriptionManager;
        }

        public Task AddAsync(RaceEvent @event)
        {
            _inMemoryRepo.Add(@event);
            if (@event.EventType != EventType.RaceFinished)
            {
                _subscriptionManager.Notify(@event.Distance, @event);
            }

            _queue.QueueAsyncTask(() => _repo.AddAsync(@event));
            return Task.CompletedTask;
        }

        public IAsyncEnumerable<RaceEvent> GetRaceEvents(Guid raceId, int distance)
        {
            //TODO
            // if (_inMemoryRepo.TryGetEvents(raceId, distance, out var events))
            // {
            //     
            // }
            // else
            // {
            //     var dbEvents = await _repo.GetRaceEvents(raceId, distance).ToListAsync();
            //     _inMemoryRepo.Fill(dbEvents);
            // }
            return _repo.GetRaceEvents(raceId, distance);
        }

        /// <summary>
        /// In memory DB is NOT using here.
        /// </summary>
        public async Task<RaceEvent?> GetLastEventByCreatorOrNull(string creatorName)
        {
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
            _inMemoryRepo.Delete(@event);
            _queue.QueueAsyncTask(() => _repo.DeleteAsync(@event));
            return Task.CompletedTask;
        }
    }
}