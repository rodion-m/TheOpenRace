using System.Collections.Generic;
using System.Collections.Immutable;
using OpenRace.Exceptions;
using OpenRace.Features.Auth;

namespace OpenRace
{
    // TODO: хранить секреты в https://docs.microsoft.com/ru-ru/aspnet/core/security/key-vault-configuration?view=aspnetcore-5.0
    public partial record AppSecrets(
        ConnectionStrings ConnectionStrings,
        YouKassaSecrets YouKassaSecrets,
        AwsSecrets AwsSecrets,
        string SentryDsn,
        AuthConfig AuthConfig
    )
    {
            public static AppSecrets GetInstance()
            {
                // if (fromEnvironment)
                // {
                //     return new AppSecrets(environment["Secrets"]);
                // }

                return DevInstance ?? throw new AppException($"{nameof(AppSecrets)} is not initialized");
            }

        private static AppSecrets? DevInstance { get; }
    }

    public record AuthConfig(Account[] Users, ImmutableHashSet<string> Admins);

    public record ConnectionStrings(string PostgreCredentials);

    public record YouKassaSecrets(string ShopId, string SecretKey);
    
    public record AwsSecrets(string AccessKey, string SecretKey);
}