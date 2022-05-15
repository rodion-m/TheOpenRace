using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using OpenRace.Data;
using OpenRace.Data.Ef;
using OpenRace.Data.GSL;
using OpenRace.Exceptions;
using OpenRace.Features.Payment;
using OpenRace.Features.Registration;

namespace OpenRace.BackgroundServices;

public class PaymentCheckingBackgroundService : BackgroundService
{
    private readonly IGenericServiceProvider<MembersRepository, PaymentService, RegistrationService> _services;

    public PaymentCheckingBackgroundService(
        IGenericServiceProvider<MembersRepository, PaymentService, RegistrationService> services)
    {
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CheckPayments();
        }
    }

    private async Task CheckPayments()
    {
        await using var scope = _services.CreateScope();
        var (membersRepository, paymentService, membersService) = scope;

        var members = await membersRepository.GetUnpaidMembers();
        foreach (var member in members)
        {
            var paid = await paymentService.IsPaymentPaid(member.Payment!.Id);
            if (paid)
            {
                await membersService.SetMembershipPaid(member);
            }
        }
    }
}