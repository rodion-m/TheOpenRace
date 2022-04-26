using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace OpenRace
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            // CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            ConfigureSerilog();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureSerilog()
        {
#if DEBUG
            const bool IsDebug = true;
#else
            const bool IsDebug = false;
#endif
            var conf = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/log_.txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Debug
                );

            if (!IsDebug)
#pragma warning disable CS0162
            {
                // ReSharper disable once HeapView.ClosureAllocation
                var secrets = AppSecrets.GetInstance();
                conf.WriteTo.Sentry(s =>
                    {
                        s.Dsn = secrets.SentryDsn;
                        s.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                        s.MinimumEventLevel = LogEventLevel.Error;
                        s.TracesSampleRate = 1.0;
                        s.Debug = true;
                    }
                );
            }
#pragma warning restore CS0162

            Log.Logger = conf.CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var secrets = AppSecrets.GetInstance();
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSentry(secrets.SentryDsn);
                });
        }
    }
}