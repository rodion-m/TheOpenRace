using BlazorDownloadFile;
using Blazored.Toast;
using BlazorPro.BlazorSize;
using BlazorTable;
using Coravel;
using EasyData.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using OpenRace.BackgroundServices;
using OpenRace.Data.Ef;
using OpenRace.Features.Auth;
using OpenRace.Features.Communication;
using OpenRace.Features.Payment;
using OpenRace.Features.Registration;
using OpenRace.Data;
using OpenRace.Data.GSL;
using OpenRace.Data.GSL.Abstractions;
using OpenRace.Features.RaceEvents;
using OpenRace.Jobs;
using OpenRace.ServicesConfigs;
using Serilog;

namespace OpenRace
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<HostOptions>(opts =>
            {
                opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            
            // services.Configure<RouteOptions>(options =>
            // {
            //     options.ConstraintMap.Add("ignoreApi", typeof(IgnoreApiRouteConstraint));
            // });

            services.AddControllers();

            var secrets = AppSecrets.GetInstance();

            // Blazor:
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddLocalization();
            services.AddBlazoredToast();
            services.AddBlazorTable();
            services.AddMediaQueryService();
            services.AddBlazorDownloadFile();

            services.ConfigureInvalidStateCustomResponse();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                        secrets.ConnectionStrings.PostgreCredentials,
                        o => o
                            .UseNodaTime()
                            .EnableRetryOnFailure()
                    )
                    .EnableSensitiveDataLogging();
            });
            services.AddDbContext<ConnectionContext>(options =>
                    options.UseNpgsql(
                        secrets.ConnectionStrings.PostgreCredentials,
                        providerOptions => providerOptions.EnableRetryOnFailure()),
                ServiceLifetime.Singleton
            );

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddQueue();
            services.AddCache();

            services.AddScheduler();
            services.AddTransient<SendRaceStartingEmailNotificationsJob>();
            services.AddTransient<SendResultsToEmailJob>();
            services.AddTransient<SendPaymentEmailNotificationsJob>();

            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddSingleton(AppConfig.Current);
            services.AddSingleton(secrets.YouKassaSecrets);
            services.AddSingleton(secrets.AwsSecrets);
            services.AddSingleton(secrets.AuthConfig);
            services.AddSingleton<ConnectionChecker>();
            services.AddSingleton<RaceEventsSubscriptionManager>();
            services.AddSingleton<RaceEventsCache>();
            services.AddSingleton<EmailTemplates>();
            services.AddSingleton<IEmailSender, AmazonSESEmailSender>();
            services.AddSingleton<EmailService, EmailService>();
            services.AddSingleton(typeof(IGenericServiceProvider<,,>), typeof(GenericServiceProvider<,,>));
            services.AddScoped<MembersRepository>();
            services.AddScoped<RaceEventsRepository>();
            services.AddScoped<RaceEventsManager>();
            services.AddScoped<RaceEventsFixer>();
            services.AddScoped(typeof(EfRepository<>));
            services.AddScoped<RegistrationService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<SessionService>();
            services.AddScoped<IMemberNumberGenerator, MemberNumberGenerator>();
            services.AddHostedService<PaymentCheckingBackgroundService>();
            services.AddHostedService<CheckConnectionBackgroundService>();
            //TODO добавить сюда запуск HostedService, который будет кешировать события для текущего raceId
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseSentryTracing();
            app.UseRequestLocalization("ru-RU");

            InitScheduler(app);

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapEasyData(options =>
                {
                    options.UseDbContext<AppDbContext>();
                });
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("{Page=Home}", "/_Host");
            });
        }

        private void InitScheduler(IApplicationBuilder app)
        {
            var provider = app.ApplicationServices;
            provider.UseScheduler(scheduler =>
            {
                //scheduler.Schedule<SendRaceStartingEmailNotificationsJob>().EveryMinute();
                //scheduler.Schedule<SendResultsToEmailJob>().EveryMinute();
                scheduler.Schedule<SendPaymentEmailNotificationsJob>().EveryMinute();
            });
        }
    }
}