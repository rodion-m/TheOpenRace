﻿using System.Globalization;
using System.Threading.Tasks;
using OpenRace.Entities;

namespace OpenRace.Features.Communication
{
    public interface IEmailService
    {
        Task SendMembershipConfirmedMessage(Member member, CultureInfo cultureInfo);
    }
}