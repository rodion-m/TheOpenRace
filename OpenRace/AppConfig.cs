using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NodaTime;
using OpenRace.Exceptions;

namespace OpenRace
{
    public record AppConfig(
        string SiteUrl,
        string Hostname,
        ZonedDateTime RaceDateTime,
        string SenderName,
        string SenderEmailAddress,
        Guid RaceId,
        CultureInfo DefaultCultureInfo,
        AppConfig.DistanceInfo[] AvailableDistances)
    {
        public string GetRaceDateTimeAsString(CultureInfo? cultureInfo = null) =>
            RaceDateTime.ToString("d MMMM yyyy HH:mm", cultureInfo ?? CultureInfo.CurrentUICulture);

        public int GetNextMemberNumber(int distance, int? currentLastNumber)
        {
            var ranges = AvailableDistances.First(it => it.DistanceMt == distance).Numbers;
            if (currentLastNumber == null)
            {
                return ranges[0].Start.Value;
            }

            var next = currentLastNumber.Value + 1;
            if (next > ranges[^1].End.Value)
            {
                throw new RegistrationNumbersEndedException(distance);
            }
            foreach (var range in ranges)
            {
                if (range.Contains(next))
                {
                    return next;
                }
                if (next < range.Start.Value)
                {
                    return range.Start.Value;
                }
            }

            throw new AppException($"Incorrect ranges: {string.Join("; ", ranges)}");
        }

        public static readonly AppConfig Current = new(
            "https://svzabeg.ru/",
            "https://panel.svzabeg.ru/",
            new ZonedDateTime(
                new LocalDate(2021, 10, 16).At(new LocalTime(14, 00, 00)),
                DateTimeZoneProviders.Tzdb["Europe/Moscow"],
                Offset.FromHours(3)),
            "Храм св. Владимира",
            "info@svzabeg.ru",
            new Guid("5a61b11d-e3ce-483b-9b8e-21387cb5c16d"),
            DefaultCultureInfo: new CultureInfo("ru"),
            AvailableDistances: new DistanceInfo[]
            {
                new(1000, new Range[] { new(1, 30), new(131, 140) }, Color.Green),
                new(2000, new Range[] { new(31, 60), new(141, 150) }, Color.Orange),
                new(5000, new Range[] { new(61, 100) }, Color.DodgerBlue),
                new(10_000, new Range[] { new(101, 130) }, Color.Red),
            }
        );

        public string GetLink(string location) => $"{Hostname}{location}";

        public record DistanceInfo(int DistanceMt, Range[] Numbers, Color Color, int OneLapDistance = 1000)
        {
            public int LapsCount => DistanceMt / OneLapDistance;
        }
    }
}