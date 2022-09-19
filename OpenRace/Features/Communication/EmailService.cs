using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
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
    public class EmailService
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
        
        public Task SendMembershipConfirmedMessage(
            Member member,
            CultureInfo cultureInfo,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(member.Email) || !EmailValidation.EmailValidator.Validate(member.Email))
            {
                throw new ArgumentException($"Некорректный email: {member.Email}");
            }
            var html = _templates.GetTemplate1Html(
                "Поздравляем!" +
                "<br/>Участие в забеге подтверждено!", 
                $"Дата и время: {_appConfig.GetRaceDateTimeAsString(cultureInfo)}", 
                $"Имя участника: {member.FullName}" 
                + $"<br/>Вы бежите под номером: {member.Number}"
                + $"<br/>Дистанция: {member.DistanceAsStringRu}",
                "Ждем вас!",
                _appConfig.SiteUrl,
                GetUnsubscribeUri(member)
            );
            return _emailSender.Send(
                $"Участник забега № {member.Number} {member.FullName}",
                html, 
          member.Email, 
                cancellationToken: cancellationToken);
        }

        private string GetUnsubscribeUri(Member member)
        {
            return _appConfig.GetLink($"unsubscribe/{member.Id}").ToString();
        }

        public Task SendPaymentNotificationMessage(Member member, CultureInfo cultureInfo)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));
            if (string.IsNullOrWhiteSpace(member.Email))
            {
                throw new InvalidOperationException($"{nameof(member)}.{nameof(member.Email)} is not set");
            }
            var html = _templates.GetTemplate1Html(
                "Напоминание" +
                "<br/>О внесении добровольного пожертвования", 
                $"Дата и время забега: {_appConfig.GetRaceDateTimeAsString(cultureInfo)}", 
                $"Имя участника: {member.FullName}" 
                + "<br/>Для участия в забеге необходимо внести добровольное пожертвование " 
                + $"до {_appConfig.GetRegistrationEndingDateTimeAsString(cultureInfo)}",
                "Ждем вас!",
                _appConfig.SiteUrl,
                GetUnsubscribeUri(member)
            );
            return _emailSender.Send(
                $"Напоминание о забеге для участника {member.FullName}", 
                html,  
                member.Email!);
        }
    }
}