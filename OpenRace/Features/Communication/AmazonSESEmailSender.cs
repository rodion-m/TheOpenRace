using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using MimeKit.Utils;

namespace OpenRace.Features.Communication
{
    public class AmazonSESEmailSender : IEmailSender
    {
        private readonly AppConfig _appConfig;
        private readonly ILogger<AmazonSESEmailSender> _logger;
        private readonly BasicAWSCredentials _credentials;

        public AmazonSESEmailSender(
            AppConfig appConfig, 
            ILogger<AmazonSESEmailSender> logger, 
            AwsSecrets awsSecrets)
        {
            _appConfig = appConfig;
            _logger = logger;
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
    }
}