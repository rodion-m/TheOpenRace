using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace OpenRace
{
    public static class AsyncLockMutexProducer<TKey> where TKey : notnull
    {
        private static readonly ConcurrentDictionary<TKey, AsyncLock> Mutexes = new();

        public static AsyncLock Get(TKey key) => Mutexes.GetOrAdd(key, _ => new AsyncLock());
        
        public static async Task Synced(TKey key, Func<Task> action)
        {
            using var mutex = await Get(key).LockAsync();
            await action();
        }
    }
}