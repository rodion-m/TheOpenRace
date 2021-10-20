using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRace.Entities;

namespace OpenRace.Features.RaceEvents
{
    public class RaceEventsSubscriptionManager
    {
        public delegate void EventChanging(RaceEvent @event);
        
        private readonly ConcurrentDictionary<int /* distance */, List<EventChanging>> _eventAddedSubscriptions = new();
        private readonly ConcurrentDictionary<int /* distance */, List<EventChanging>> _eventDeletedSubscriptions = new();
        private readonly ConcurrentDictionary<(int, int), Action> _allEventsUpdatedSubscriptions = new();
        private readonly object _mutex = new();
        
        public void OnAllEventsShouldBeUpdated()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var listener in _allEventsUpdatedSubscriptions.Values)
            {
                listener();
            }
        }
        
        
        public int Subscribe(int distance, EventChanging onEventAdded, EventChanging onEventDeleted, Action? onAllEventsUpdated)
        {
            lock (_mutex)
            {
                _eventAddedSubscriptions.AddOrUpdate(distance,
                    _ => new List<EventChanging>() { onEventAdded },
                    (_, list) =>
                    {
                        list.Add(onEventAdded);
                        return list;
                    });
                
                _eventDeletedSubscriptions.AddOrUpdate(distance,
                    _ => new List<EventChanging>() { onEventDeleted },
                    (_, list) =>
                    {
                        list.Add(onEventDeleted);
                        return list;
                    });

                var index = _eventAddedSubscriptions[distance].Count;

                if (onAllEventsUpdated != null)
                {
                    _allEventsUpdatedSubscriptions.TryAdd((distance, index), onAllEventsUpdated);
                }
                return index;
            }
        }

        public void Unsubscribe(int distance, int index)
        {
            if(index < 0) return;
            lock (_mutex)
            {
                DeleteEvent(_eventAddedSubscriptions, distance, index);
                DeleteEvent(_eventDeletedSubscriptions, distance, index);
                _allEventsUpdatedSubscriptions.TryRemove((distance, index), out _);
            }
        }

        private void DeleteEvent(ConcurrentDictionary<int, List<EventChanging>> subscriptions, int distance, int index)
        {
            subscriptions.AddOrUpdate(distance, 
                _ => new List<EventChanging>(), 
                (_, list) =>
                {
                    if (index < list.Count)
                    {
                        list.RemoveAt(index);
                    }

                    return list;
                });
        }

        public void NotifyEventAdded(RaceEvent @event)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            NotifyListeners(_eventAddedSubscriptions, @event);
        }

        public void NotifyEventDeleted(RaceEvent @event)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            NotifyListeners(_eventDeletedSubscriptions, @event);
        }
        
        private void NotifyListeners(ConcurrentDictionary<int,List<EventChanging>> subscriptions, RaceEvent @event)
        {
            if (subscriptions.TryGetValue(@event.Distance, out var listeners))
            {
                foreach (var listener in listeners.ToArray())
                {
                    listener(@event);
                }
            }
        }
    }
}