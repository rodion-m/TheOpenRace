using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenRace.Data.Ef;
using OpenRace.Data.Specifications;
using OpenRace.Entities;
using Yandex.Checkout.V3;

namespace OpenRace.Features.Payment
{
    public class PaymentService
    {
        private readonly MembersRepository _members;
        private readonly AsyncClient _youKassClient;

        public PaymentService(
            YouKassaSecrets youKassaSecrets, MembersRepository members)
        {
            ArgumentNullException.ThrowIfNull(youKassaSecrets);
            _members = members ?? throw new ArgumentNullException(nameof(members));
            var client = new Client(youKassaSecrets.ShopId, youKassaSecrets.SecretKey);
            _youKassClient = client.MakeAsync();
        }

        public Yandex.Checkout.V3.Payment DecodeWebhookRequest(
            string requestMethod, string requestContentType, Stream requestBody)
        {
            if (requestMethod == null) throw new ArgumentNullException(nameof(requestMethod));
            if (requestContentType == null) throw new ArgumentNullException(nameof(requestContentType));
            if (requestBody == null) throw new ArgumentNullException(nameof(requestBody));
            
            var message = Client.ParseMessage(requestMethod, requestContentType, requestBody);
            if (message == null)
            {
                throw new NullReferenceException(nameof(message));
            }
            var payment = message.Object;
            return payment;
            
            // if (message?.Event == Event.PaymentWaitingForCapture && payment.Paid)
            // {
            //     Log($"Got message: payment.id={payment.Id}, payment.paid={payment.Paid}");
            //
            //     // 4. Подтвердите готовность принять платеж
            //     await _asyncClient.CapturePaymentAsync(payment.Id);
            // }
        }



        public async Task<(Entities.Payment payment, Uri redirectUri)> CreatePayment(
            decimal amount, string hostUrl, string? hash = null)
        {
            hash ??= Guid.NewGuid().ToString();
            var newPayment = new NewPayment
            {
                Amount = new Amount { Value = amount },
                Confirmation = new Confirmation 
                { 
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = CreateReturnPageUri(hash, hostUrl).ToString()
                },
                Capture = true,
                Description = "Взнос за участие в благотворительном забеге"
            };
            var payment = await _youKassClient.CreatePaymentAsync(newPayment);

            var racePayment = new Entities.Payment(payment.Id, payment.Amount.Value, hash);
            return (racePayment, new Uri(payment.Confirmation.ConfirmationUrl));
        }

        public Uri CreateReturnPageUri(string hash, string hostUrl) 
            => new Uri($"{hostUrl}purchase/{hash}");
        
        public async Task<Uri> GetOrCreatePayment(Member member, string hostUrl, decimal donation)
        {
            ArgumentNullException.ThrowIfNull(hostUrl);
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (member.Payment is { } payment)
            {
                var p = await _youKassClient.GetPaymentAsync(payment.Id);
                return new Uri(p.Confirmation.ConfirmationUrl);
            }
            return await ReCreatePayment(member, hostUrl, donation);
        }
        
        public async Task<Uri> ReCreatePayment(Member member, string hostUrl, decimal donation)
        {
            ArgumentNullException.ThrowIfNull(hostUrl);
            if (member == null) throw new ArgumentNullException(nameof(member));
            var (payment, redirectUri) = await CreatePayment(
                donation,
                hostUrl
            );
            member.Payment = payment;
            await _members.UpdateAsync(member);
            return redirectUri;
        }

        public async Task<Member?> FindMemberByPaymentHash(string paymentHash)
        {
            if (paymentHash == null) throw new ArgumentNullException(nameof(paymentHash));
            var member = await _members.FirstOrDefaultAsync(new MemberByPaymentHash(paymentHash));
            return member;
        }
        
        public async Task<bool> IsPaymentPaid(string paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _youKassClient.GetPaymentAsync(paymentId, cancellationToken);
            return payment.Paid;
        }
    }
}