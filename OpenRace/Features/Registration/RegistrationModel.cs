using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OpenRace.Entities;

namespace OpenRace.Features.Registration
{
    // https://docs.google.com/forms/d/e/1FAIpQLSfr7m732GHkF9UWSQFjP72lqY80iGtmza_dHJs57K4swGF-uA/formResponse
    public record RegistrationModel
    {
        // [Required]
        // [Display(Name = "Сумма пожертвования")]
        // [FromForm(Name = "donation")]
        // public decimal Donation { get; set; }
        
        [Required]
        [Display(Name = "Имя")]
        [FromForm(Name = "first_name")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Фамилия")]
        [FromForm(Name = "last_name")]
        public string? LastName { get; set; }

        [Display(Name = "Отчество")]
        [FromForm(Name = "patronymic_name")]
        public string? PatronymicName { get; set; }

        [Required]
        [Display(Name = "Возраст")]
        [Range(7, 120)]
        [FromForm(Name = "age")] 
        public int Age { get; set; }
        
        [Required, EnumDataType(typeof(Gender))]
        [Display(Name = "Пол")]
        [FromForm(Name = "gender")]
        public string? Gender { get; set; }
        
        [Required]
        [Display(Name = "Email")]
        [EmailValidation.Email]
        [FromForm(Name = "email")] 
        public string? Email { get; set; }
        
        [Display(Name = "Телефон")]
        //[Phone]
        [FromForm(Name = "phone")] 
        public string? Phone { get; set; }
        
        [Required]
        [Display(Name = "Дистанция")]
        [FromForm(Name = "distance")]
        public string? Distance { get; set; }

        [Display(Name = "Как узнали о забеге")]
        [FromForm(Name = "referer")]
        public string? Referer { get; set; }
    }
}