using NodaTime;

namespace OpenRace.Extensions
{
    public static class NodaTimeX
    {
        public static bool IsEqualAccurateToMinute(this LocalDateTime a, LocalDateTime b)
        {
            return a.Date == b.Date && a.Hour == b.Hour && a.Minute == b.Minute;
        }
    }
}