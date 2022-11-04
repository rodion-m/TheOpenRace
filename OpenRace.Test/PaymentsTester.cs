using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenRace.BackgroundServices;
using OpenRace.Data.Ef;
using OpenRace.Features.Payment;
using OpenRace.Features.Registration;

namespace OpenRace.Test;

public class PaymentsTester
{
    public async void Test()
    {
        // var logger = new Logger<PaymentCheckingBackgroundService>(new LoggerFactory());
        // var ЯЩИК = new ServiceCollection()
        //     .AddDbContext<AppDbContext>()
        //     .AddSingleton<PaymentService>()
        //     .AddScoped<Random>()
        //     .BuildServiceProvider();
        // var service = new PaymentCheckingBackgroundService(ЯЩИК, logger);
        // await service.StartAsync(default);
    }
}