using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRace.Data.Ef
{
    public class ConnectionChecker
    {
        private readonly ConnectionContext _dbContext;

        public ConnectionChecker(ConnectionContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> CanConnect(CancellationToken cancellationToken = default) 
            => _dbContext.Database.CanConnectAsync(cancellationToken);

        public async Task<TimeSpan> Ping(bool warmUp = true, CancellationToken cancellationToken = default)
        {
            if (warmUp)
            {
                await CanConnect(cancellationToken);
            }
            var stopwatch = Stopwatch.StartNew();
            await CanConnect(cancellationToken);
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}