using System.Threading.Tasks;
using OpenRace.Entities;

namespace OpenRace.Features.Registration;

public interface IMemberNumberGenerator
{
    Task<int> GetNewMemberNumber(Member member);
    bool ShouldResetMemberNumber(Member existedMember, Member newMember);
}