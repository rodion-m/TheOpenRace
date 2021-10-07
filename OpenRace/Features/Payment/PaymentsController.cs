using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRace.Features.Communication;
using OpenRace.Features.Registration;
using Serilog;
using Yandex.Checkout.V3;

namespace OpenRace.Features.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IEmailService _emailService;
        private readonly RegistrationService _registrationService;

        public PaymentsController(
            PaymentService paymentService, ILogger<PaymentsController> logger, IEmailService emailService, RegistrationService registrationService)
        {
            _paymentService = paymentService;
            _logger = logger;
            _emailService = emailService;
            _registrationService = registrationService;
        }
        
        [HttpPost("Notify")]
        public async Task<IActionResult> Notify([FromQuery] string pwd)
        {
            //Webhook
            if (pwd != "0ac8a951b5e842eb8798e55624ce2927")
            {
                _logger.LogWarning("Incorrect webhook pwd: {Pwd}", pwd);
                return Forbid();
            }
            var body = new byte[Request.ContentLength ?? 0];
            await Request.Body.ReadAsync(body);
            var payment = _paymentService.DecodeWebhookRequest(
                Request.Method, Request.ContentType, new MemoryStream(body));
            if (payment.Paid)
            {
                var member = await _registrationService.SetMembershipPaid(payment.Id);
                await _emailService.SendMembershipConfirmedMessage(member);
            }
            else
            {
                _logger.LogWarning("Payment is not paid: {PaymentId}", payment.Id);
            }

            return Ok();
        }
    }
}