using System;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using EmailValidation;
using Microsoft.Extensions.Logging;
using NodaTime;
using OpenRace.Data.Ef;
using OpenRace.Extensions;
using OpenRace.Features.Communication;

namespace OpenRace.Jobs
{
    public class SendResultsToEmailJob : IInvocable
    {
        private readonly IEmailSender _mailService;
        private readonly MembersRepository _repo;
        private readonly IClock _clock;
        private readonly AppConfig _appConfig;
        private readonly ILogger<SendResultsToEmailJob> _logger;

        public SendResultsToEmailJob(
            IEmailSender mailService, 
            MembersRepository repo, 
            IClock clock, 
            AppConfig appConfig, 
            ILogger<SendResultsToEmailJob> logger
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
            var sendEmailsAt = new LocalDate(2021, 11, 8).At(new LocalTime(15, 12));
            var now = _clock.GetCurrentInstant().InZone(_appConfig.RaceStartTime.Zone).LocalDateTime;
            if (!now.IsEqualAccurateToMinute(sendEmailsAt))
            {
                return;
            }
            var allMembers = await _repo.GetSubscribedMembers().ToListAsync();
            var members = allMembers.DistinctBy(it => it.Email)
                .Where(it => it.Email != null && EmailValidator.Validate(it.Email))
                .ToArray();
            _logger.LogInformation("Start sending emails to {Count} users", members.Length);
            foreach(var member in members)
            {
                try
                {
                    //TODO вынести отправку в EmailService
                    var htmlBody = _messageHtml.Replace("{{unsubscribeUri}}", _appConfig.GetLink($"unsubscribe/{member.Id}"));
                    await _mailService.Send("Подводим итоги забега в Перово", htmlBody, member.Email!);
                    _logger.LogInformation("Email sent to {Email}", member.Email);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while sending an email to {Email}", member.Email);
                }
            }        
        }

        private const string _messageHtml = "<div>\n<p>Доброе утро!</p>\n<p>Друзья, эти соревнования мы проводили с использованием новой программы по хронометражу. Поэтому в результатах могли возникнуть неточности. Мы понимаем, что спортсмены, прошедшие дистанцию и потратившие силы на соревнование серьёзно относятся к полученным результатам. После забега мы учли ошибки и исправили их. Надеемся на понимание и приносим свои извинения. Уверены, что следующий наш забег будет проведён более качественно. Те участники, которые не получили свои награды за 1, 2, 3 места (классификация на каждой дистанции, мужчины, женщины, мальчики, девочки) согласно результатам&nbsp; опубликованным на сайте и спортсменам, не занявшим призовые места и не получившим медаль за участие в забеге просьба обратиться по телефону: +79645522761 Дмитрий.&nbsp; Получить свою награду можно по адресу ул. Перовская 66к3.</p>\n<p>Результаты забега опубликованы на сайте: <a href=\"https://svzabeg.ru/\">https://svzabeg.ru/</a></p>\n<p>А еще Вы нам очень поможете если поделитесь своим мнением о прошедшем забеге, пройдя по этой ссылке: <a href=\"https://forms.gle/NdPeBKvHHQVrRoCdA\">https://forms.gle/NdPeBKvHHQVrRoCdA</a></p>\n</div>Будьте здоровы, занимайтесь спортом и до встречи на новом забеге!<br/><br/>---<br/>\nЕсли вы не хотите больше получать от нас актуальная информацию, вы можете <a href=\"{{unsubscribeUri}}\" target=\"_blank\" rel=\"noopener\">отказаться</a> от сообщений.";
    }
}