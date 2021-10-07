using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NodaTime;
using OpenRace.Data.Ef;
using OpenRace.Data.Specifications;
using OpenRace.Entities;
using OpenRace.Features.Payment;
using OpenRace.Data;

namespace OpenRace.Features.Registration
{
    public class RegistrationService
    {
        private readonly MembersRepository _members;
        private readonly PaymentService _paymentService;
        private readonly IClock _clock;

        public RegistrationService(MembersRepository members, PaymentService paymentService, IClock clock)
        {
            _members = members;
            _paymentService = paymentService;
            _clock = clock;
        }

        public async Task<(RegistrationResult, Member)> RegisterOrUpdate(Member member)
        {
            var existedMember = await _members.FirstOrDefaultAsync(
                new MemberByEmailAndName(member.Email, member.FullName));

            if (existedMember != null)
            {
                member = member with { Id = existedMember.Id };
                await _members.DeleteAsync(existedMember);
            }

            await _members.AddAsync(member);
            return (existedMember != null ? RegistrationResult.Registered : RegistrationResult.Updated, member);
        }

        public async Task<string> RegisterAndCreatePayment(RegistrationModel model, string hostUrl)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var hash = Guid.NewGuid().ToString();
            var (payment, redirectUri) = await _paymentService.CreatePayment(
                500, //model.Donation
                hash,
                hostUrl
            );
            await RegisterMember(model, payment);
            return redirectUri;
        }

        public async Task<(RegistrationResult, Member)> RegisterMember(RegistrationModel model, Entities.Payment? payment)
        {
            var member = CreateMemberFromRegistrationModel(model, payment);
            using var locking = await _memberNumberMutex.LockAsync();
            if (payment == null)
            {
                member.Number = await GetNextMemberNumber();
            }

            return await RegisterOrUpdate(member);
        }

        private Member CreateMemberFromRegistrationModel(RegistrationModel model, Entities.Payment? payment)
        {
            model.Phone.TryStandardizePhoneNumber(out var phone);
            var member = new Member(
                Guid.NewGuid(),
                _clock.GetCurrentInstant(),
                $"{model.LastName} {model.FirstName} {model.PatronymicName}",
                model.Email!,
                phone,
                model.Age,
                Enum.Parse<Gender>(model.Gender!),
                int.Parse(model.Distance!) * 1000,
                model.Referer
            )
            {
                Payment = payment
            };
            return member;
        }

        public async Task<Member> SetMembershipPaid(string paymentId)
        {
            var member = await _members.FirstAsync(new MemberByPaymentId(paymentId));
            await SetMembershipPaid(member);
            return member;
        }

        private readonly AsyncLock _memberNumberMutex = new AsyncLock();

        public async Task<Member> SetMembershipPaid(Member member, bool assignNumberIfItsNot = true)
        {
            using var locking = await _memberNumberMutex.LockAsync();
            member.Payment!.PaidAt = _clock.GetCurrentInstant();
            if (assignNumberIfItsNot && member.Number == null)
            {
                member.Number = await GetNextMemberNumber();
            }

            await _members.UpdateAsync(member);
            return member;
        }

        public async Task AssignNewMemberNumber(Member member)
        {
            member.Number = await GetNextMemberNumber();
            await _members.UpdateAsync(member);
        }
        
        [Pure]
        private async Task<int> GetNextMemberNumber()
        {
            var lastMember = await _members.GetLastPaidMemberOrNull();
            return lastMember != null ? lastMember.Number.GetValueOrDefault() + 1 : 1;
        }
    }

    public enum RegistrationResult
    {
        Registered,
        Updated
    }
}