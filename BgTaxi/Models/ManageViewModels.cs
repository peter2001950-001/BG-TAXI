using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using BgTaxi.Models.Models;
using BgTaxi.Models.Models;
using System.Web.Mvc;

namespace BgTaxi.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class IndexCompanyViewModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "Име на фирмата")]
        public string Name { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "ЕИК")]
        public string EIK { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "ДДС №")]
        public string DDS { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Адрес")]
        public string Address { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Материално отговорно лице")]
        public string MOL { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Уникален код")]
        public string UniqueNumber { get; set; }

    }

    public class DriversCompanyViewModel
    {
        public List<Driver> Drivers { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class DriverBasicInfo
    {
        public string UserId { get; set; }
        public string Name { get; set; }
    }

    public class CarsCompanyViewModel
    {
        public class DriverAndUser
        {
            public int DriverId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

        }
        

        public Dictionary<Car, DriverBasicInfo> CarsAndDrivers { get; set; }
        public SelectList Drivers { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Регистрационен номер")]
        public string RegisterNumber { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Марка")]
        public string Brand { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Модел")]
        public string Model { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Година")]
        public int Year { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Вътрешен номер")]
        public string InternalNumber { get; set; }
        public int CarId { get; set; }
        public string SelectedDriver { get; set; }

    }



    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}