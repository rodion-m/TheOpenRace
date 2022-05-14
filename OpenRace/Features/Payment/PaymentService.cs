using System;
using System.IO;
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
        private readonly AsyncClient _asyncClient;

        public PaymentService(
            YouKassaSecrets youKassaSecrets, MembersRepository members)
        {
            _members = members;
            var client = new Client(youKassaSecrets.ShopId, youKassaSecrets.SecretKey);
            _asyncClient = client.MakeAsync();
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
            decimal amount, string hash, string hostUrl)
        {
            var newPayment = new NewPayment
            {
                Amount = new Amount { Value = amount },
                Confirmation = new Confirmation 
                { 
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = CreateReturnPageUri(hash, hostUrl)
                },
                Capture = true,
                Description = "Взнос за участие в благоритворительном забеге"
            };
            var payment = await _asyncClient.CreatePaymentAsync(newPayment);

            var racePayment = new Entities.Payment(payment.Id, payment.Amount.Value, hash);
            return (racePayment, new Uri(payment.Confirmation.ConfirmationUrl));
        }

        public string CreateReturnPageUri(string hash, string hostUrl) 
            => $"{hostUrl}purchase/{hash}";

        public async Task<Uri> ReCreatePayment(Member member, string hostUrl, decimal donation)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var hash = Guid.NewGuid().ToString();
            var (payment, redirectUri) = await CreatePayment(
                donation,
                hash,
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
        
        public async Task<bool> IsPaymentPaid(string paymentId)
        {
            var payment = await _asyncClient.GetPaymentAsync(paymentId);
            return payment.Paid;
        }
    }
}