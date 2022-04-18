using System;
using EasyData.EntityFrameworkCore;
using NodaTime;

namespace OpenRace.Entities
{
    [MetaEntity(DisplayName = "Результат участника", DisplayNamePlural = "Результаты", Description = "Результаты забега")]
    public record MemberResult : IEntity
    {
        public MemberResult(Guid Id, Guid RaceId, Guid MemberId, Instant Result, Guid CreatedBy, int MemberAge, int Distance)
        {
            this.Id = Id;
            this.RaceId = RaceId;
            this.MemberId = MemberId;
            this.Result = Result;
            this.CreatedBy = CreatedBy;
            this.MemberAge = MemberAge;
            this.Distance = Distance;
        }

        public Guid Id { get; init; }
        public Guid RaceId { get; set; }
        public Guid MemberId { get; set; }
        public Instant Result { get; set; }
        public Guid CreatedBy { get; set; }
        public int MemberAge { get; set; }
        public int Distance { get; set; }
    }
}