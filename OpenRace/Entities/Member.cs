using System;
using EasyData.EntityFrameworkCore;
using NodaTime;

namespace OpenRace.Entities
{
    [MetaEntity(DisplayName = "Участник", DisplayNamePlural = "Участники", Description = "Список участников")]
    public record Member : IEntity
    {
        public Member(
            Guid id, 
            Instant createdAt,
            string fullName,
            string email,
            string? phone, 
            int age, 
            Gender gender, 
            int distanceMt, 
            string? referer)
        {
            Id = id;
            CreatedAt = createdAt;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Age = age;
            Gender = gender;
            DistanceMt = distanceMt;
            Referer = referer;
        }

        public Guid Id { get; set; }
        public Instant CreatedAt { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        
        /// <summary> Дистанция в метрах </summary>
        public int DistanceMt { get; set; }
        
        public string? Referer { get; set; }

        public int? Number { get; set; }
        
        public Duration? RaceResult { get; set; }
        
        public bool Subscribed { get; set; } = true;
        
        //https://entityframeworkcore.com/knowledge-base/53063181/ef-core-database-specific-columns-to-nested-object
        [MetaEntityAttr(Enabled = false)]
        public Payment? Payment { get; set; }

        public bool IsChild()
        {
            return Age < 14;
        }
    }
    
    public enum Gender
    {
        Male, Female
    }
}