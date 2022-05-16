using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

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
            var conf = ConfigureLogger(new LoggerConfiguration());
            Log.Logger = conf.CreateLogger();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var secrets = AppSecrets.GetInstance();
            return Host.CreateDefaultBuilder(args)
                .UseSerilog(configureLogger: (hostingContext, conf) =>
                {
                    ConfigureLogger(conf);
                    conf.ReadFrom.Configuration(hostingContext.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSentry(secrets.SentryDsn);
                });
        }

        private static LoggerConfiguration ConfigureLogger(LoggerConfiguration conf)
        {
            const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
            conf
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File(
                    outputTemplate: outputTemplate, //new CompactJsonFormatter(),
                    path: "logs/log_.txt",
                    rollingInterval: RollingInterval.Day
                );
#if !DEBUG
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
#endif
            return conf;
        }
    }
}