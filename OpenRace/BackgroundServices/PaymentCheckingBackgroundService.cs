using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using OpenRace.Data;
using OpenRace.Data.Ef;
using OpenRace.Data.GSL;
using OpenRace.Entities;
using OpenRace.Exceptions;
using OpenRace.Features.Payment;
using OpenRace.Features.Registration;
using Polly;

namespace OpenRace.BackgroundServices;

public class PaymentCheckingBackgroundService : BackgroundService
{
    private readonly IGenericServiceProvider<MembersRepository, PaymentService, RegistrationService> _services;
    private readonly ILogger<PaymentCheckingBackgroundService> _logger;

    public PaymentCheckingBackgroundService(
        IGenericServiceProvider<MembersRepository, PaymentService, RegistrationService> services,
        ILogger<PaymentCheckingBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CheckPayments();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while checking payments");
            }
        }
    }

    private async Task CheckPayments()
    {
        await using var scope = _services.CreateScope();
        var (membersRepository, paymentService, membersService) = scope;
        var members = await membersRepository.GetUnpaidMembers();
        foreach (var member in members)
        {
            var paid = await IsPaymentPaidWithRetry(member, paymentService);
            if (paid)
            {
                await membersService.SetMembershipPaid(member);
            }
        }
    }

    private async Task<bool> IsPaymentPaidWithRetry(Member member, PaymentService paymentService)
    {
        var paymentId = member.Payment!.Id;
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                (exception, _) =>
                {
                    _logger.LogWarning(exception, "Error occured while checking payment {PaymentId}", paymentId);
                });
        var res = await policy.ExecuteAndCaptureAsync(() => paymentService.IsPaymentPaid(paymentId));
        if (res.Outcome == OutcomeType.Failure)
        {
            _logger.LogError(
                res.FinalException,
                "Error occured while checking payment {PaymentId}, MemberId: {MemberId}",
                paymentId, member.Id
            );
        }

        return res.Result;
    }
}