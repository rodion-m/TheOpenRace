using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using MimeKit.Utils;
using OpenRace.Entities;
using Serilog;

namespace OpenRace.Features.Communication
{
    public class AmazonSESEmailService : IEmailService
    {
        private readonly AppConfig _appConfig;
        private readonly ILogger<AmazonSESEmailService> _logger;
        private readonly EmailTemplates _templates;
        private readonly BasicAWSCredentials _credentials;

        public AmazonSESEmailService(
            AppConfig appConfig, 
            ILogger<AmazonSESEmailService> logger, 
            EmailTemplates templates,
            AwsSecrets awsSecrets)
        {
            _appConfig = appConfig;
            _logger = logger;
            _templates = templates;
            _credentials = new BasicAWSCredentials(awsSecrets.AccessKey, awsSecrets.SecretKey);
        }
        public async Task Send(string subject, string htmlBody, string receiver)
        {
            using var client = new AmazonSimpleEmailServiceClient(_credentials, RegionEndpoint.EUNorth1);
            var sender = $"{_appConfig.SenderName} <{_appConfig.SenderEmailAddress}>";
            var sendRequest = new SendEmailRequest
            {
                Source = Encoding.UTF8.GetString(Rfc2047.EncodeText(Encoding.UTF8, sender)),
                Destination = new Destination
                {
                    ToAddresses = new List<string> { receiver }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = htmlBody
                        }
                    }
                },
                // If you are not using a configuration set, comment
                // or remove the following line 
                //ConfigurationSetName = configSet
            };
            try
            {
                _ = await client.SendEmailAsync(sendRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending email to {Subject}", subject);
            }
        }

        public Task SendMembershipConfirmedMessage(Member member, CultureInfo cultureInfo)
        {
            var html = _templates.GetTemplate1Html(
                "Участие в забеге подтверждено!", 
                $"Дата и время: {_appConfig.GetRaceDateTimeAsString(cultureInfo)}", 
                $"Имя участника: {member.FullName}<br/>Вы бежите под номером: {member.Number}",
                "Ждем вас!",
                _appConfig.SiteUrl,
                _appConfig.GetLink($"unsubscribe/{member.Id}")
            );
            return Send($"Участник забега № {member.Number} {member.FullName}", html,  member.Email);
        }
    }
}