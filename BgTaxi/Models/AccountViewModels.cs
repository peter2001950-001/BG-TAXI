using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BgTaxi.Models
{
    // Models returned by AccountController actions.
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Hometown")]
        public string Hometown { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }

        [Display(Name = "Запомни ме?")]
        public bool RememberMe { get; set; }


    }

  

    public class RegisterCompanyViewModel
    {

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Повтори паролата")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Паролите не съвпада!")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Мобилен Телефон")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Име на фирмата")]
        public string CompanyName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "ЕИК")]
        public string EIK { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "ДДС Номер")]
        public string DDS { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Адрес")]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Материялна отговорно лице (МОЛ)")]
        public string MOL { get; set; }



    }
    public class RegisterEmployeeViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Потвърди парола")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Паролите не съвпада!")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Мобилен Телефон")]
        public string PhoneNumber { get; set; }

        public SelectList Employee { get; set; }

        public string SelectedEmployee { get; set; }

        [Required]
        [Display(Name = "Уникален номер на фирмата")]
        public string UniqueNumber { get; set; }

    }
   

    public class RegisterClientViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Потвърди парола")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Паролите не съвпада!")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Мобилен Телефон")]
        public string PhoneNumber { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
