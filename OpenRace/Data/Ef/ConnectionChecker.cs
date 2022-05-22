﻿using System;
using System.Diagnostics;
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

        public Task<bool> CanConnect() => _dbContext.Database.CanConnectAsync();

        public async Task<TimeSpan> Ping()
        {
            var stopwatch = Stopwatch.StartNew();
            await CanConnect();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}