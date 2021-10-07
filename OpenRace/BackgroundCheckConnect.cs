using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenRace.Data.Ef;
using Serilog;

namespace OpenRace
{
    public class BackgroundCheckConnect : BackgroundService
    {
        private readonly ConnectionChecker _checker;
        private readonly ILogger<BackgroundCheckConnect> _logger;

        public BackgroundCheckConnect(ConnectionChecker checker, ILogger<BackgroundCheckConnect> logger)
        {
            _checker = checker;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var canConnect = await _checker.CanConnect();
            if (canConnect)
            {
                var stopwatch = Stopwatch.StartNew();
                await _checker.CanConnect();
                stopwatch.Stop();
                _logger.LogInformation("!!! Db connected. Ping: {Ping} ms. !!!", stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogCritical("!!! DB IS NOT CONNECTED !!!");
            }
        }
    }
}