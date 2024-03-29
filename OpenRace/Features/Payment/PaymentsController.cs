﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRace.Features.Registration;

namespace OpenRace.Features.Payment
{
    //https://host/api/payments/notify?pwd=0ac8a951b5e842eb8798e55624ce2927
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly RegistrationService _registrationService;

        public PaymentsController(
            PaymentService paymentService,
            ILogger<PaymentsController> logger,
            RegistrationService registrationService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
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
            _ = await Request.Body.ReadAsync(body);
            var payment = _paymentService.DecodeWebhookRequest(
                Request.Method, Request.ContentType!, new MemoryStream(body));
            if (payment.Paid)
            {
                await _registrationService.SetMembershipPaid(payment.Id);
            }
            else
            {
                _logger.LogWarning("Payment is not paid: {PaymentId}", payment.Id);
            }

            return Ok();
        }
    }
}