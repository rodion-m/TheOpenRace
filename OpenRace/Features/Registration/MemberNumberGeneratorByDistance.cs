using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenRace.Data.Ef;
using OpenRace.Entities;

namespace OpenRace.Features.Registration;

public class MemberNumberGeneratorByDistance : IMemberNumberGenerator
{
    private readonly MembersRepository _members;
    private readonly AppConfig _appConfig;

    public MemberNumberGeneratorByDistance(MembersRepository members, AppConfig appConfig)
    {
        _members = members;
        _appConfig = appConfig;
    }
    
    [Pure]
    public async Task<int> GetNewMemberNumber(Member member, CancellationToken cancellationToken = default)
    {
        var lastMember = await _members.GetLastMemberNumberByDistance(member.Distance, cancellationToken);
        return _appConfig.GetNextMemberNumber(member.Distance, lastMember?.Number);
    }

    public bool ShouldResetMemberNumber(Member existedMember, Member newMember)
    {
        return newMember.Distance != existedMember.Distance;
    }
}