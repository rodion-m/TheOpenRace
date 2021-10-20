using System;
using EasyData.EntityFrameworkCore;
using NodaTime;

namespace OpenRace.Entities
{
    [MetaEntity(DisplayName = "Событие забега", DisplayNamePlural = "События забега", Description = "Событие забега")]
    public record RaceEvent : IEntity
    {
        public RaceEvent(Guid id, Guid raceId, int memberNumber, EventType eventType, Instant timeStamp, string creatorName, int distance)
        {
            Id = id;
            RaceId = raceId;
            MemberNumber = memberNumber;
            EventType = eventType;
            TimeStamp = timeStamp;
            CreatorName = creatorName;
            Distance = distance;
        }

        public Guid Id { get; set; }
        public Guid RaceId { get; set; }
        public int MemberNumber { get; set; }
        public EventType EventType { get; set; }
        public Instant TimeStamp { get; set; }
        public string CreatorName { get; set; }
        public int Distance { get; set; }
    }

    public enum EventType
    {
        CameToTheRace,
        RaceStarted,
        LapComplete,
        RaceFinished
    }
}