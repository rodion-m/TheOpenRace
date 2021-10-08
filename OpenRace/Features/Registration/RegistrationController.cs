using System.Linq;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using OpenRace.Features.Communication;

namespace OpenRace.Features.Registration
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly RegistrationService _registrationService;
        private readonly IEmailService _emailService;
        private readonly IQueue _queue;
        private readonly AppConfig _appConfig;

        public RegistrationController(
            RegistrationService registrationService, 
            IEmailService emailService, 
            IQueue queue, 
            AppConfig appConfig)
        {
            _registrationService = registrationService;
            _emailService = emailService;
            _queue = queue;
            _appConfig = appConfig;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }
        
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegistrationModel model)
        {
            var distance = int.Parse(model.DistanceKm!) * 1000;
            if (!_appConfig.AvailableDistances.Any(it => it.DistanceMt == distance))
            {
                return BadRequest($"Неизвестная дистанция: {model.DistanceKm}");
            }
            var (_, member) = await _registrationService.RegisterMember(model, null);
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            _queue.QueueAsyncTask(() => _emailService.SendMembershipConfirmedMessage(member, culture));
            var redirectUri = $"{Request.Scheme}://{Request.Host}/confirmed/{member.Id}";
            return Redirect(redirectUri);
        }

    }
}