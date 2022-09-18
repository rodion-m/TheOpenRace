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
        private readonly AppConfig _appConfig;

        public RegistrationController(
            RegistrationService registrationService,
            AppConfig appConfig)
        {
            _registrationService = registrationService;
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
            var distance = int.Parse(model.DistanceMt!);
            if (_appConfig.AvailableDistances.All(it => it.DistanceMt != distance))
            {
                return BadRequest($"Неизвестная дистанция: {model.DistanceMt}");
            }

            // var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>()!;
            // var culture = rqf.RequestCulture.Culture;
            // var hostUrl = $"{Request.Scheme}://{Request.Host}/";
            var redirectUri = await _registrationService.RegisterOrUpdate(model);
            
            if (!decimal.TryParse(model.Donation, out _))
            {
                return BadRequest("Сумма пожертвования должна быть задана и иметь корректное значение");
            }

            return Redirect(redirectUri.ToString());
        }
    }
}