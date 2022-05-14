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
    public async Task<int> GetNewMemberNumber(Member member)
    {
        var lastMember = await _members.GetLastMemberNumberByDistance(member.Distance);
        return _appConfig.GetNextMemberNumber(member.Distance, lastMember?.Number);
    }
}