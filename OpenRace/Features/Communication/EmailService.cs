using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using MimeKit.Utils;
using OpenRace.Entities;
using OpenRace.Exceptions;
using Serilog;

namespace OpenRace.Features.Communication
{
    public class EmailService : IEmailService
    {
        private readonly AppConfig _appConfig;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplates _templates;

        public EmailService(
            AppConfig appConfig, 
            IEmailSender emailSender,
            EmailTemplates templates)
        {
            _appConfig = appConfig;
            _emailSender = emailSender;
            _templates = templates;
        }
        
        public Task SendMembershipConfirmedMessage(Member member, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(member.Email) || !EmailValidation.EmailValidator.Validate(member.Email))
            {
                throw new ArgumentException($"Некорректный email: {member.Email}");
            }
            var html = _templates.GetTemplate1Html(
                "Поздравляем!" +
                "<br/>Участие в забеге подтверждено!", 
                $"Дата и время: {_appConfig.GetRaceDateTimeAsString(cultureInfo)}", 
                $"Имя участника: {member.FullName}<br/>Вы бежите под номером: {member.Number}",
                "Ждем вас!",
                _appConfig.SiteUrl,
                _appConfig.GetLink($"unsubscribe/{member.Id}")
            );
            return _emailSender.Send($"Участник забега № {member.Number} {member.FullName}", html,  member.Email);
        }
    }
}