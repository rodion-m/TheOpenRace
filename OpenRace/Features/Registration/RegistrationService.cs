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
        private readonly EmailService _emailService;
        private readonly IQueue _queue;
        private readonly IMemberNumberGenerator _memberNumberGenerator;

        public RegistrationService(
            MembersRepository members,
            PaymentService paymentService, 
            IClock clock,
            AppConfig appConfig, 
            EmailService emailService, 
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


        public async Task<Uri> RegisterOrUpdate(RegistrationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (!decimal.TryParse(model.Donation, out var donation))
            {
                throw new InvalidOperationException($"{nameof(model.Donation)} is not decimal ({model.Donation})");
            }
            
            var newMember = CreateMemberFromRegistrationModel(model);
            var existedMember = await _members.FirstOrDefaultAsync(
                new MemberByEmailAndName(newMember.Email, newMember.FullName));
            
            Uri? redirectUri;
            if (existedMember is not null)
            {
                (newMember, redirectUri) = await UpdateMember(existedMember, newMember, donation);
            }
            else
            {
                redirectUri = await RegisterMember(newMember, donation);
            }

            if (!_appConfig.PaymentRequired && newMember.Number == null)
            {
                await AssignNewMemberNumber(newMember);
            }
            if (!_appConfig.PaymentRequired)
            {
                if (newMember.Email != null && EmailValidator.Validate(newMember.Email))
                {
                    _queue.QueueAsyncTask(() 
                        => _emailService.SendMembershipConfirmedMessage(newMember, _appConfig.DefaultCultureInfo));
                }
            }

            redirectUri ??= _appConfig.GetConfirmedPageUri(newMember.Id);
            
            return redirectUri;
        }

        private async Task<(Member member, Uri? redirectUri)> UpdateMember(
            Member existedMember, Member newMember, decimal donation)
        {
            newMember = newMember with
            {
                Id = existedMember.Id,
                Number = existedMember.Number,
                Payment = existedMember.Payment
            };
            if (_memberNumberGenerator.ShouldResetMemberNumber(existedMember, newMember))
            {
                newMember.Number = null;
            }
            
            Uri? redirectUri = null;
            if (_appConfig.PaymentRequired && existedMember.Payment?.PaidAt == null)
            {
                (newMember.Payment, redirectUri) = await _paymentService.CreatePayment(donation, _appConfig.Host);
            }
            
            await _members.DeleteAsync(existedMember);
            await _members.AddAsync(newMember);
            return (newMember, redirectUri);
        }

        private async Task<Uri?> RegisterMember(Member member, decimal donation)
        {
            Uri? redirectUri = null;
            if (_appConfig.PaymentRequired)
            {
                (member.Payment, redirectUri) = await _paymentService.CreatePayment(donation, _appConfig.Host);
            }
            await _members.AddAsync(member);
            return redirectUri;
        }
        
        private Member CreateMemberFromRegistrationModel(RegistrationModel model)
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
            );
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