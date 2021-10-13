using System;
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
using OpenRace.Data.Ef;
using OpenRace.Features.Auth;
using OpenRace.Features.Communication;
using OpenRace.Features.Payment;
using OpenRace.Features.Registration;
using OpenRace.Data;
using OpenRace.ServicesConfigs;
using Sentry.AspNetCore;
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
            var secrets = AppSecrets.GetInstance();

            // Blazor:
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddLocalization();
            services.AddBlazoredToast();
            services.AddBlazorTable();
            services.AddMediaQueryService();

            services.ConfigureInvalidStateCustomResponse();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    secrets.ConnectionStrings.PostgreCredentials,
                    o => o
                        .UseNodaTime()
                        .EnableRetryOnFailure()
                )
                .LogTo(Log.Debug)
                .EnableSensitiveDataLogging()
            );
            services.AddDbContext<ConnectionContext>(options =>
                    options.UseNpgsql(
                        secrets.ConnectionStrings.PostgreCredentials,
                        providerOptions => providerOptions.EnableRetryOnFailure()),
                ServiceLifetime.Singleton
            );

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddQueue();
            services.AddCache();

            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddSingleton(AppConfig.Current);
            services.AddSingleton(secrets.YouKassaSecrets);
            services.AddSingleton(secrets.AwsSecrets);
            services.AddSingleton<ConnectionChecker>();
            services.AddSingleton<EmailTemplates>();
            services.AddSingleton<IEmailService, AmazonSESEmailService>();
            services.AddScoped<MembersRepository>();
            services.AddScoped<EventsRepository>();
            services.AddScoped(typeof(EfRepository<>));
            services.AddScoped<RegistrationService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<SessionService>();
            services.AddHostedService<BackgroundCheckConnect>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseRouting();
            app.UseSerilogRequestLogging();
            app.UseSentryTracing();
            app.UseRequestLocalization("ru-RU");

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapEasyData(options => {
                    options.UseDbContext<AppDbContext>();
                });
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
        }
    }
}