using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRace.Entities;

namespace OpenRace.Features.Events
{
    public class EventsSubscriptionManager
    {
        public delegate void EventChanged(RaceEvent @event);
        
        private readonly ConcurrentDictionary<int /* distance */, List<EventChanged>> _subscriptions = new();

        public void Notify(int distance, RaceEvent @event)
        {
            if (_subscriptions.TryGetValue(distance, out var listeners))
            {
                foreach (var listener in listeners.ToArray())
                {
                    listener(@event);
                }
            }
        }
        
        public int Subscribe(int distance, EventChanged action)
        {
            _subscriptions.AddOrUpdate(distance, 
                _ => new List<EventChanged>() { action }, 
                (_, list) =>
            {
                list.Add(action);
                return list;
            });
            
            return _subscriptions[distance].Count;
        }

        public void Unsubscribe(int distance, int index)
        {
            if(index < 0) return;
            _subscriptions.AddOrUpdate(distance, _ => new List<EventChanged>(), (_, list) =>
            {
                if (index < list.Count)
                {
                    list.RemoveAt(index);
                }

                return list;
            });
        }
    }
}