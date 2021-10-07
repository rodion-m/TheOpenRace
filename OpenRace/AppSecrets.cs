namespace OpenRace
{
    // TODO: https://docs.microsoft.com/ru-ru/aspnet/core/security/key-vault-configuration?view=aspnetcore-5.0
    public partial record AppSecrets(
        ConnectionStrings ConnectionStrings,
        YouKassaSecrets YouKassaSecrets,
        AwsSecrets AwsSecrets,
        string SentryDsn
    )
    {
            public static AppSecrets GetInstance()
            {
                // if (fromEnvironment)
                // {
                //     return new AppSecrets(environment["JwtSecrets"]);
                // }

                return DevInstance ?? throw new AppException($"{nameof(AppSecrets)} is not initialized");
            }

        private static AppSecrets? DevInstance { get; }
    }

    public record ConnectionStrings(string PostgreCredentials);

    public record YouKassaSecrets(string ShopId, string SecretKey);
    
    public record AwsSecrets(string AccessKey, string SecretKey);
}