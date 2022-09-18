using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OpenRace.Entities;

namespace OpenRace.Features.Registration
{
    // https://docs.google.com/forms/d/e/1FAIpQLSfr7m732GHkF9UWSQFjP72lqY80iGtmza_dHJs57K4swGF-uA/formResponse
    public record RegistrationModel
    {
        [Required]
        [Display(Name = "Сумма пожертвования")]
        [FromForm(Name = "donation")]
        [Range(10, 1_000_000)]
        public string Donation { get; set; } = null!;
        
        [Required]
        [Display(Name = "Имя")]
        [FromForm(Name = "first_name")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Фамилия")]
        [FromForm(Name = "last_name")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Display(Name = "Отчество")]
        [FromForm(Name = "patronymic_name")]
        [MaxLength(50)]
        public string? PatronymicName { get; set; }

        [Required, Range(1, 120)]
        [Display(Name = "Возраст")]
        [FromForm(Name = "age")] 
        public int Age { get; set; }
        
        [Required, EnumDataType(typeof(Gender))]
        [Display(Name = "Пол")]
        [FromForm(Name = "gender")]
        public string? Gender { get; set; }
        
        //[Required]
        [Display(Name = "Email")]
        //[EmailValidation.Email]
        [FromForm(Name = "email")]
        [MaxLength(50)]
        public string? Email { get; set; }
        
        [Display(Name = "Телефон")]
        [FromForm(Name = "phone")]
        [MaxLength(50)]
        public string? Phone { get; set; }
        
        [Required, Range(0, int.MaxValue)]
        [Display(Name = "Дистанция")]
        [FromForm(Name = "distance")]
        public string? DistanceMt { get; set; }

        [Display(Name = "Как узнали о забеге")]
        [FromForm(Name = "referer")]
        [MaxLength(200)]
        public string? Referer { get; set; }
        
        [Display(Name = "Регистратор")]
        [FromForm(Name = "registeredBy")]
        [MaxLength(200)]
        public string? RegisteredBy { get; set; }
        
        [Display(Name = "Имя родителя")]
        [FromForm(Name = "parent_first_name")]
        [MaxLength(50)]
        public string? ParentFirstName { get; set; }

        [Display(Name = "Фамилия родителя")]
        [FromForm(Name = "parent_last_name")]
        [MaxLength(50)]
        public string? ParentLastName { get; set; }

        [Display(Name = "Отчество родителя")]
        [FromForm(Name = "parent_patronymic_name")]
        [MaxLength(50)]
        public string? ParentPatronymicName { get; set; }
        
        [Display(Name = "Округ участника")]
        [FromForm(Name = "region")]
        [MaxLength(50)]
        public string? Region { get; set; }
        
        [Display(Name = "Район участника")]
        [FromForm(Name = "district")]
        [MaxLength(50)]
        public string? District { get; set; }
    }
}