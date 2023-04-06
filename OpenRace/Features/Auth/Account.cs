namespace OpenRace.Features.Auth
{
    public record Account(string UserName, string Password)
    {
        public static readonly Account Empty = new ("", "");

        public bool IsValid 
            => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);
    }
}