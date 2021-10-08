using System.Globalization;
using System.Threading.Tasks;
using OpenRace.Entities;

namespace OpenRace.Features.Communication
{
    public interface IEmailService
    {
        Task Send(string subject, string htmlBody, string receiver);
        Task SendMembershipConfirmedMessage(Member member, CultureInfo cultureInfo);
    }
}