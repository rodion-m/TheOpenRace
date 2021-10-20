using System.Threading.Tasks;
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

        public async Task AddEvent(RaceEvent @event)
        {
            await _inMemoryRepo.AddEvent(@event);
            _subscriptionManager.Notify(@event.Distance, @event);
            _queue.QueueAsyncTask(() => _repo.AddAsync(@event));
        }
    }
}