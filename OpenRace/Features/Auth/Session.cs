using System.Threading.Tasks;

namespace OpenRace.Features.Auth
{
    public record Session(string UserName, string Password)
    {
        public static readonly Session Empty = new ("", "");

        public bool IsValid => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);
        
        public bool IsAuthorized() => Password == "zabeg481516";

        public bool IsAdmin() => IsAuthorized() && UserName == "Rodion" || UserName == "DimaAdmin";
    }
}