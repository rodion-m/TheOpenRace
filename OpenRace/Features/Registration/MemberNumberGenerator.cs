using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenRace.Data.Ef;
using OpenRace.Entities;

namespace OpenRace.Features.Registration;

public class MemberNumberGenerator : IMemberNumberGenerator
{
    private readonly MembersRepository _members;
    private readonly AppConfig _appConfig;

    public MemberNumberGenerator(MembersRepository members, AppConfig appConfig)
    {
        _members = members;
        _appConfig = appConfig;
    }
    
    [Pure]
    public async Task<int> GetNewMemberNumber(Member member)
    {
        var lastMember = await _members.GetLastMemberNumber();
        return (lastMember?.Number ?? 0) + 1;
    }
}