using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using OpenRace.Entities;

namespace OpenRace.Features.Communication
{
    public interface IEmailSender
    {
        Task Send(string subject, string htmlBody, string receiver, CancellationToken cancellationToken = default);
    }
}