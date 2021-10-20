﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using EmailValidation;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NodaTime;
using OpenRace.Data.Ef;
using OpenRace.Features.Communication;

namespace OpenRace.Jobs
{
    public class SendEmailNotificationJob : IInvocable
    {
        private readonly IEmailService _mailService;
        private readonly MembersRepository _repo;
        private readonly IClock _clock;
        private readonly AppConfig _appConfig;
        private readonly ILogger<SendEmailNotificationJob> _logger;

        public SendEmailNotificationJob(
            IEmailService mailService, 
            MembersRepository repo, 
            IClock clock, 
            AppConfig appConfig, 
            ILogger<SendEmailNotificationJob> logger
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
            var sendEmailsAt = _appConfig.RaceDateTime.Date.At(new LocalTime(9, 0));
            var now = _clock.GetCurrentInstant().InZone(_appConfig.RaceDateTime.Zone);
            if (now.Date != sendEmailsAt.Date || now.Hour != sendEmailsAt.Hour || now.Minute != sendEmailsAt.Minute) // TODO
            {
                return;
            }
            var members = await _repo.AllAsync().ToListAsync();
            foreach(var member in members.Where(it => it.Email != null && EmailValidator.Validate(it.Email)).Shuffle())
            {
                try
                {
                    await _mailService.SendMembershipConfirmedMessage(member, _appConfig.DefaultCultureInfo);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while sending an email to {Email}", member.Email);
                }
            }        
        }
    }
}