using System;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using EmailValidation;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NodaTime;
using OpenRace.Data.Ef;
using OpenRace.Extensions;
using OpenRace.Features.Communication;

namespace OpenRace.Jobs
{
    public class SendPaymentEmailNotificationsJob : IInvocable
    {
        private readonly IEmailService _mailService;
        private readonly MembersRepository _repo;
        private readonly IClock _clock;
        private readonly AppConfig _appConfig;
        private readonly ILogger<SendPaymentEmailNotificationsJob> _logger;

        public SendPaymentEmailNotificationsJob(
            IEmailService mailService, 
            MembersRepository repo, 
            IClock clock, 
            AppConfig appConfig, 
            ILogger<SendPaymentEmailNotificationsJob> logger
            )
        {
            _mailService = mailService;
            _repo = repo;
            _clock = clock;
            _appConfig = appConfig;
            _logger = logger;
        }

        public async Task Invoke()
        {
            var sendEmailsAt = _appConfig.PaymentNotificationSendingTime;
            var now = _clock.GetCurrentInstant().InZone(_appConfig.RaceStartTime.Zone).LocalDateTime;
            if (!now.IsEqualAccurateToMinute(now.Date.At(sendEmailsAt)))
            {
                return;
            }
            var members = await _repo.GetSubscribedMembers().ToListAsync();
            members = members.Where(it => it.Payment!.PaidAt == null)
                .Where(it => it.CreatedAt > _clock.GetCurrentInstant().Minus(Duration.FromHours(5)))
                //.Where(it => it.PaymentNotificationSentAt == null) // TODO
                .ToList();
            
            foreach(var member in members.Where(it => it.Email != null && EmailValidator.Validate(it.Email)).Shuffle())
            {
                try
                {
                    //await _mailService.SendPaymentNotificationMessage(member, _appConfig.DefaultCultureInfo);
                    //member.PaymentNotificationSentAt = _clock.GetCurrentInstant();
                    await _repo.UpdateAsync(member);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while sending an email to {Email}", member.Email);
                }
            }        
        }
    }
}