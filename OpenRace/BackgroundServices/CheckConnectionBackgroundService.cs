using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenRace.Data.Ef;

namespace OpenRace.BackgroundServices
{
    public class CheckConnectionBackgroundService : BackgroundService
    {
        private readonly ConnectionChecker _checker;
        private readonly ILogger<CheckConnectionBackgroundService> _logger;

        public CheckConnectionBackgroundService(ConnectionChecker checker, ILogger<CheckConnectionBackgroundService> logger)
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