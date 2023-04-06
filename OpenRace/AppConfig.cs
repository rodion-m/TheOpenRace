using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NodaTime;
using OpenRace.Exceptions;
using OpenRace.Extensions;

namespace OpenRace
{
    public record AppConfig(
        string Title,
        string SiteUrl,
        string Host,
        ZonedDateTime RaceStartsAt,
        ZonedDateTime RegistrationEndsAt,
        LocalDateTime NotifyMemberAt,
        LocalTime PaymentNotificationSendingTime,
        string SenderName,
        string SenderEmailAddress,
        Guid RaceId,
        CultureInfo DefaultCultureInfo,
        bool PaymentRequired,
        AppConfig.DistanceInfo[] AvailableDistances,
        Duration MinLapDuration
    )
    {
        public static readonly AppConfig Current;

        static AppConfig()
        {
            var timeZone = DateTimeZoneProviders.Tzdb["Europe/Moscow"];
            var raceStartsAt = new ZonedDateTime(
                new LocalDate(2022, 10, 15)
                    .At(new LocalTime(14, 00, 00)),
                timeZone,
                Offset.FromHours(3)
            );
            var registrationEndsAt = new ZonedDateTime(
                new LocalDate(2022, 10, 14)
                    .At(new LocalTime(18, 00, 00)),
                timeZone,
                Offset.FromHours(3)
            );
            
            Current = new AppConfig(
                "Забег в Перово", 
                "https://svzabeg.ru/", 
                "https://perovo-zabeg.azurewebsites.net/", //"https://panel.svzabeg.ru/",
                raceStartsAt,
                registrationEndsAt,
                raceStartsAt.Date.At(new LocalTime(9, 0)),
                new LocalTime(12, 0),
                "Фонд храма св. Владимира",
                "info@svzabeg.ru",
                new Guid("C82422B4-FD01-483E-8641-B3992C973F0E"),
                DefaultCultureInfo: new CultureInfo("ru"),
                true,
                AvailableDistances: new DistanceInfo[]
                {
                    new(1050,
                        new Range[] { new(1, 30), new(131, 140), new(201, 250) },
                        Color.Green,
                        "1 километр (для детей): \"семейная\""
                    ),
                    new(2100,
                        new Range[] { new(31, 60), new(301, 350) },
                        Color.Orange,
                        "2 километра: \"я попробую\""
                    ),
                    new(5250,
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
            RaceStartsAt.ToString("d MMMM yyyy HH:mm", cultureInfo ?? DefaultCultureInfo);
        
        public string GetRegistrationEndingDateTimeAsString(CultureInfo? cultureInfo = null) =>
            RegistrationEndsAt.ToString("d MMMM yyyy HH:mm", cultureInfo ?? DefaultCultureInfo);

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

        public Uri GetLink(string location) => new($"{Host}{location}");
        public Uri GetLink(params string[] uriSegments) => new($"{Host}/{string.Join("/", uriSegments)}");

        public record DistanceInfo(
            int DistanceMt, 
            Range[] Numbers, 
            Color Color, 
            string? Name,
            int OneLapDistance = 1050)
        {
            public int LapsCount => DistanceMt / OneLapDistance;
            public string DistanceAsStringRu => DistanceInMetersToStringRu(DistanceMt);

            public static string DistanceInMetersToStringRu(int distance)
            {
                var km = distance / 1000;
                var mt = distance % 1000;
                return mt == 0 ? $"{km} км." : $"{km} км. {mt} м.";
            }
        }

        public DistanceInfo GetDistanceInfo(int distance) 
            => AvailableDistances.First(it => it.DistanceMt == distance);

        public Uri GetConfirmedPageUri(Guid memberId) 
            => GetLink($"confirmed/{memberId}");
    }
}