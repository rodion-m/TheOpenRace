using System.Collections.Concurrent;
using Nito.AsyncEx;

namespace OpenRace
{
    public static class AsyncLockMutexProducer<TKey> where TKey : notnull
    {
        private static readonly ConcurrentDictionary<TKey, AsyncLock> _mutexes = new();

        public static AsyncLock Get(TKey key) => _mutexes.GetOrAdd(key, _ => new AsyncLock());
    }
}