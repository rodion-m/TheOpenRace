using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        public RegistrationController(RegistrationService registrationService, IEmailService emailService)
        {
            _registrationService = registrationService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }
        
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegistrationModel model)
        {
            var (_, member) = await _registrationService.RegisterMember(model, null);
            await _emailService.SendMembershipConfirmedMessage(member);
            var redirectUri = $"{Request.Scheme}://{Request.Host}/confirmed/{member.Id}";
            return Redirect(redirectUri);
        }

    }
}