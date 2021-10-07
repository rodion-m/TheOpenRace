using System.Globalization;
using NodaTime;

namespace OpenRace
{
    public record AppConfig(
        string SiteUrl,
        string Hostname,
        ZonedDateTime RaceDateTime,
        string SenderName,
        string SenderEmailAddress,
        int[] AvailableDistances)
    {
        public string RaceDateTimeAsString
            => RaceDateTime.ToString("d MMMM yyyy 'г.' HH:mm", CultureInfo.CurrentUICulture);

        public static readonly AppConfig Current = new(
            "https://svzabeg.ru/",
            "https://panel.svzabeg.ru/",
            new ZonedDateTime(
                new LocalDate(2021, 10, 16).At(new LocalTime(13, 00, 00)),
                DateTimeZoneProviders.Tzdb["Europe/Moscow"],
                Offset.FromHours(3)),
            "Храм св. Владимира",
            "info@svzabeg.ru",
            AvailableDistances: new[] { 1000, 3000, 5000, 10_000 }
        );

        public string GetLink(string location) => $"{Hostname}{location}";
    }
}