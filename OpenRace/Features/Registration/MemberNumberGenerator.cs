using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenRace.Data.Ef;
using OpenRace.Entities;

namespace OpenRace.Features.Registration;

public class MemberNumberGenerator : IMemberNumberGenerator
{
    private readonly MembersRepository _members;

    public MemberNumberGenerator(MembersRepository members, AppConfig appConfig)
    {
        _members = members;
    }
    
    [Pure]
    public async Task<int> GetNewMemberNumber(Member member)
    {
        var lastMember = await _members.GetLastMemberNumber();
        return (lastMember?.Number ?? 0) + 1;
    }

    public bool ShouldResetMemberNumber(Member newMember, Member existedMember) => false;
}