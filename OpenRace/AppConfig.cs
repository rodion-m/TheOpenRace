using System.Drawing;
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
        CultureInfo DefaultCultureInfo,
        AppConfig.DistanceInfo[] AvailableDistances)
    {
        public string GetRaceDateTimeAsString(CultureInfo? cultureInfo = null) =>
            RaceDateTime.ToString("d MMMM yyyy 'г.' HH:mm", cultureInfo ?? CultureInfo.CurrentUICulture);

        public static readonly AppConfig Current = new(
            "https://svzabeg.ru/",
            "https://panel.svzabeg.ru/",
            new ZonedDateTime(
                new LocalDate(2021, 10, 16).At(new LocalTime(14, 00, 00)),
                DateTimeZoneProviders.Tzdb["Europe/Moscow"],
                Offset.FromHours(3)),
            "Храм св. Владимира",
            "info@svzabeg.ru",
            DefaultCultureInfo: new CultureInfo("ru"),
            AvailableDistances: new DistanceInfo[]
            {
                new(1000, 1, Color.Green),
                new(2000, 31, Color.Orange),
                new(5000, 61, Color.DodgerBlue),
                new(10_000, 101, Color.Red),
            }
        );

        public string GetLink(string location) => $"{Hostname}{location}";

        public record DistanceInfo(int DistanceMt, int BeginsWithNumber, Color Color, int OneLapDistance = 1000)
        {
            public int LapsCount => DistanceMt / OneLapDistance;
        }
    }
}