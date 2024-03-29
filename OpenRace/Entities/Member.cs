﻿using System;
using EasyData.EntityFrameworkCore;
using NodaTime;

namespace OpenRace.Entities
{
    [MetaEntity(DisplayName = "Участник", DisplayNamePlural = "Участники", Description = "Список участников")]
    public record Member : IEntity
    {
        protected Member()
        {
        }
        public Member(
            Guid id, 
            Instant createdAt,
            string fullName,
            string? email,
            string? phone, 
            int age, 
            Gender gender, 
            int distance, 
            string? referer, 
            string? registeredBy, 
            string? parentName,
            string? region,
            string? district)
        {
            Id = id;
            CreatedAt = createdAt;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Age = age;
            Gender = gender;
            Distance = distance;
            Referer = referer;
            RegisteredBy = registeredBy;
            ParentName = parentName;
            Region = region;
            District = district;
        }

        public const int AdultsAge = 17;

        [MetaEntityAttr(Index = int.MaxValue, Editable = false, ShowOnView = false)]
        public Guid Id { get; init; }
        
        [MetaEntityAttr(Enabled = false)]
        public Instant CreatedAt { get; set; }

        [MetaEntityAttr(Index = 1)]
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        
        /// <summary> Дистанция в метрах </summary>
        public int Distance { get; set; }
        
        public string? Referer { get; set; }

        [MetaEntityAttr(Index = 0, Sorting = 1)]
        public int? Number { get; set; }
        
        
        [MetaEntityAttr(Enabled = false)]
        public Duration? RaceResult { get; set; }
        
        public bool Subscribed { get; set; } = true;
        
        public string? RegisteredBy { get; set; }
        
        [MetaEntityAttr(Enabled = false)]
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        
        public string? Region { get; set; }
        
        public string? District { get; set; }
        
        [MetaEntityAttr(Enabled = false)]
        public Instant? PaymentNotificationSentAt { get; set; }

        public Guid PaymentId { get; set; }
        
        //https://entityframeworkcore.com/knowledge-base/53063181/ef-core-database-specific-columns-to-nested-object
        [MetaEntityAttr(Enabled = false)]
        public Payment? Payment { get; set; }

        public string DistanceAsStringRu 
            => AppConfig.DistanceInfo.DistanceInMetersToStringRu(Distance);
        
        public bool IsChild() => Age < AdultsAge;
        public bool IsAdult() => Age >= AdultsAge;
    }
    
    public enum Gender
    {
        Male, Female
    }
}