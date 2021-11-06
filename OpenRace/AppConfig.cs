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
        ZonedDateTime RaceStartTime,
        LocalDateTime NotifyMemberAt,
        string SenderName,
        string SenderEmailAddress,
        Guid RaceId,
        CultureInfo DefaultCultureInfo,
        AppConfig.DistanceInfo[] AvailableDistances,
        Duration MinLapDuration)
    {
        public static readonly AppConfig Current;

        static AppConfig()
        {
            var raceStartTime = new ZonedDateTime(
                new LocalDate(2021, 10, 16).At(new LocalTime(14, 00, 00)),
                DateTimeZoneProviders.Tzdb["Europe/Moscow"],
                Offset.FromHours(3)
            );
            Current = new AppConfig(
                "https://svzabeg.ru/",
                "https://panel.svzabeg.ru/",
                raceStartTime,
                raceStartTime.Date.At(new LocalTime(9, 0)),
                "Храм св. Владимира",
                "info@svzabeg.ru",
                new Guid("2a61b11d-e3ce-483b-9b8e-21387cb5c16d"),
                DefaultCultureInfo: new CultureInfo("ru"),
                AvailableDistances: new DistanceInfo[]
                {
                    new(1000,
                        new Range[] { new(1, 30), new(131, 140), new(201, 250) },
                        Color.Green,
                        "1 километр (для детей): \"семейная\""
                    ),
                    new(2000,
                        new Range[] { new(31, 60), new(301, 350) },
                        Color.Orange,
                        "2 километра: \"я попробую\""
                    ),
                    new(5000,
                        new Range[] { new(61, 100), new(501, 550) },
                        Color.DodgerBlue,
                        "5 километров: \"я смогу\""
                    ),
                    new(10_000,
                        new Range[] { new(101, 130) },
                        Color.Red,
                        "10 километров: \"профи\""
                    ),
                },
                Duration.FromMinutes(3)
            );
        }

        public string GetRaceDateTimeAsString(CultureInfo? cultureInfo = null) =>
            RaceStartTime.ToString("d MMMM yyyy HH:mm", cultureInfo ?? CultureInfo.CurrentUICulture);

        public int GetNextMemberNumber(int distance, int? currentLastNumber)
        {
            var ranges = GetDistanceInfo(distance).Numbers;
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

        public string GetLink(string location) => $"{Hostname}{location}";

        public record DistanceInfo(int DistanceMt, Range[] Numbers, Color Color, string? Name,
            int OneLapDistance = 1000)
        {
            public int LapsCount => DistanceMt / OneLapDistance;
            public int DistanceKm => DistanceMt / 1000;
        }

        public DistanceInfo GetDistanceInfo(int distance) => AvailableDistances.First(it => it.DistanceMt == distance);
    }
}