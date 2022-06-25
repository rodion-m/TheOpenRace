using System.Threading;
using System.Threading.Tasks;
using OpenRace.Entities;

namespace OpenRace.Features.Registration;

public interface IMemberNumberGenerator
{
    Task<int> GetNewMemberNumber(Member member, CancellationToken cancellationToken = default);
    bool ShouldResetMemberNumber(Member existedMember, Member newMember);
}