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
using OpenRace.Data.GSL.Abstractions;
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
        var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CheckPayments(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.LogError(e, "Error while checking payments");
            }
        }
    }

    private async Task CheckPayments(CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateScope();
        var (membersRepository, paymentService, membersService) = scope;
        var members = await membersRepository.GetUnpaidMembers(cancellationToken);
        foreach (var member in members)
        {
            var paid = await IsPaymentPaidWithRetry(member, paymentService, cancellationToken);
            if (paid)
            {
                await membersService.SetMembershipPaid(member, cancellationToken: cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private async Task<bool> IsPaymentPaidWithRetry(
        Member member, PaymentService paymentService, CancellationToken cancellationToken)
    {
        var paymentId = member.Payment!.Id;
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                (exception, _) =>
                {
                    _logger.LogWarning(exception, "Error occured while checking payment {PaymentId}", paymentId);
                });
        var res = await policy.ExecuteAndCaptureAsync(
            ct => paymentService.IsPaymentPaid(paymentId, ct), cancellationToken);
        if (res.Outcome == OutcomeType.Failure && res.FinalException is not OperationCanceledException)
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