using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using EmailValidation;
using Nito.AsyncEx;
using NodaTime;
using OpenRace.Data.Ef;
using OpenRace.Data.Specifications;
using OpenRace.Entities;
using OpenRace.Features.Payment;
using OpenRace.Data;
using OpenRace.Extensions;
using OpenRace.Features.Communication;

namespace OpenRace.Features.Registration
{
    public class RegistrationService
    {
        private readonly MembersRepository _members;
        private readonly PaymentService _paymentService;
        private readonly IClock _clock;
        private readonly AppConfig _appConfig;
        private readonly IEmailService _emailService;
        private readonly IQueue _queue;
        private readonly IMemberNumberGenerator _memberNumberGenerator;

        public RegistrationService(
            MembersRepository members,
            PaymentService paymentService, 
            IClock clock,
            AppConfig appConfig, 
            IEmailService emailService, 
            IQueue queue,
            IMemberNumberGenerator memberNumberGenerator)
        {
            _members = members;
            _paymentService = paymentService;
            _clock = clock;
            _appConfig = appConfig;
            _emailService = emailService;
            _queue = queue;
            _memberNumberGenerator = memberNumberGenerator;
        }

        private static readonly AsyncLock _registrationMutex = new();

        public async Task<(RegistrationResult, Member)> RegisterOrUpdate(Member member)
        {
            using var locking = await _registrationMutex.LockAsync();
            var existedMember = await _members.FirstOrDefaultAsync(
                new MemberByEmailAndName(member.Email, member.FullName));

            if (existedMember != null)
            {
                member = member with
                {
                    Id = existedMember.Id
                };
                if (member.Distance == existedMember.Distance)
                {
                    member.Number = existedMember.Number;
                }

                await _members.DeleteAsync(existedMember);
            }

            await _members.AddAsync(member);
            return (existedMember != null ? RegistrationResult.Registered : RegistrationResult.Updated, member);
        }

        public async Task<Uri> Register(
            RegistrationModel model, string hostUrl, CultureInfo cultureInfo, bool payNow)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (hostUrl == null) throw new ArgumentNullException(nameof(hostUrl));
            if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));

            if (!decimal.TryParse(model.Donation, out var donation))
            {
                throw new InvalidOperationException($"{nameof(model.Donation)} is not decimal ({model.Donation})");
            }
            
            var paymentHash = Guid.NewGuid().ToString();
            Entities.Payment? payment = null;
            Uri? redirectUri = null;
            if (payNow)
            {
                (payment, redirectUri) = await _paymentService.CreatePayment(
                    donation,
                    paymentHash,
                    hostUrl
                );
            }

            var (_, member) = await RegisterMember(model, payment);
            // if (member.Email != null && EmailValidator.Validate(member.Email))
            // {
            //     _queue.QueueAsyncTask(() => _emailService.SendMembershipConfirmedMessage(member, cultureInfo));
            // }

            redirectUri ??= _appConfig.GetConfirmedPageUri(hostUrl, member.Id);
            return redirectUri;
        }

        private async Task<(RegistrationResult, Member)> RegisterMember(
            RegistrationModel model,
            Entities.Payment? payment)
        {
            var member = CreateMemberFromRegistrationModel(model, payment);
            using var locking = await _memberNumberMutex.LockAsync();
            if (payment == null)
            {
                member.Number = await _memberNumberGenerator.GetNewMemberNumber(member);
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
                int.Parse(model.DistanceKm!) * 1000,
                model.Referer,
                model.RegisteredBy,
                $"{model.ParentLastName} {model.ParentFirstName} {model.ParentPatronymicName}",
                model.Region,
                model.District
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

        private static readonly AsyncLock _memberNumberMutex = new();

        public async Task<Member> SetMembershipPaid(
            Member member, bool assignNumberIfItsNot = true, bool notifyByEmail = true)
        {
            using var locking = await _memberNumberMutex.LockAsync();
            member.Payment!.PaidAt = _clock.GetCurrentInstant();
            if (assignNumberIfItsNot && member.Number == null)
            {
                member.Number = await _memberNumberGenerator.GetNewMemberNumber(member);
            }

            await _members.UpdateAsync(member);

            if (notifyByEmail)
            {
                if (member.Email != null && EmailValidator.Validate(member.Email))
                {
                    await _emailService.SendMembershipConfirmedMessage(member, _appConfig.DefaultCultureInfo);
                }
            }
            
            return member;
        }

        public async Task AssignNewMemberNumber(Member member)
        {
            member.Number = await _memberNumberGenerator.GetNewMemberNumber(member);
            await _members.UpdateAsync(member);
        }
    }

    public enum RegistrationResult
    {
        Registered,
        Updated
    }
}