using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BgTaxi.Models;
using BgTaxi.Models.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace BgTaxi.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

      private Models.Models.Database db = new Models.Models.Database();

        public ManageController()
        {
            
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.ProfileInfoUpdateSucess ? "Профилът е обновен успешно."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };

            if (User.IsInRole("Company")) {

                var company = db.Companies.Where(x => x.UserId == userId).First();
                var viewModel = new IndexCompanyViewModel { UniqueNumber = company.UniqueNumber, Name = company.Name, MOL = company.MOL, Address = company.Address, DDS = company.DDS, EIK = company.EIK };

                return View("IndexCompany", viewModel);
            }
            else if(User.IsInRole("Driver"))
            {
                return View("IndexDriver", model);

            }else if (User.IsInRole("Dispatcher"))
            {
                UserManager<Models.ApplicationUser> userManager = new UserManager<Models.ApplicationUser>(new UserStore<Models.ApplicationUser>(new Models.ApplicationDbContext()));
                var user = userManager.FindById(User.Identity.GetUserId());
                var viewModel = new IndexDispatcherViewModel();
                viewModel.FirstName = user.FirstName;
                viewModel.LastName = user.LastName;
                viewModel.Telephone = user.PhoneNumber;
                var dispatcher = db.Dispatchers.Where(x => x.UserId == user.Id).Include(x=>x.Company).First();
                viewModel.CompanyName = dispatcher.Company.Name;

                return View("IndexDispatcher", viewModel);
            }
            else
            {
                return View("IndexDriver", model);
            }

        }

        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCompanyInfo(IndexCompanyViewModel viewModel)
        {
           var company =  db.Companies.Where(x => x.UniqueNumber == viewModel.UniqueNumber).First();
            company.Name = viewModel.Name;
            company.MOL = viewModel.MOL;
            company.EIK = viewModel.EIK;
            company.DDS = viewModel.DDS;
            company.Address = viewModel.Address;
            db.SaveChanges();
            var newViewModel = new IndexCompanyViewModel() { Address = company.Address, DDS = company.DDS, EIK = company.EIK, MOL = company.MOL, UniqueNumber = company.UniqueNumber, Name = company.Name };


            return RedirectToAction("Index", new { message = ManageMessageId.ProfileInfoUpdateSucess });
        }
        [HttpPost]
        [Authorize(Roles = "Dispatcher")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDispatcherInfo(IndexDispatcherViewModel viewModel)
        {
            var store = new UserStore<Models.ApplicationUser>(new Models.ApplicationDbContext());
            UserManager<Models.ApplicationUser> userManager = new UserManager<Models.ApplicationUser>(store);
            var user = userManager.FindById(User.Identity.GetUserId());
            user.PhoneNumber = viewModel.Telephone;
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            userManager.Update(user);
            var context = store.Context;
            context.SaveChanges();


            var newViewModel = new IndexDispatcherViewModel() {  FirstName = user.FirstName, Telephone = user.Telephone, LastName = user.LastName, CompanyName = viewModel.CompanyName};


            return RedirectToAction("Index", new { message = ManageMessageId.ProfileInfoUpdateSucess });
        }



        [HttpGet]
        [Authorize(Roles ="Company, Driver")]
        public ActionResult Cars()
        {

            
            if (User.IsInRole("Company"))
            {
                var currentUserId = User.Identity.GetUserId();
                var cars = db.Cars.Where(x => x.Company.UserId == currentUserId).ToList();
                var drivers = new List<Driver>();
                foreach (var car in cars)
                {
                    drivers.Add(db.Drivers.Where(x => x.Car.Id == car.Id).FirstOrDefault());
                }
                var listUsers = new List<DriverBasicInfo>();
                foreach (var driver in drivers)
                {
                    if (driver != null)
                    {
                        var userFound = UserManager.FindById(driver.UserId);
                        listUsers.Add(new DriverBasicInfo() { UserId = userFound.Id, Name = string.Format("{0} {1}", userFound.FirstName, userFound.LastName) });
                    }else
                    {
                        listUsers.Add(new DriverBasicInfo() { Name = "Няма", UserId = "0"});
                    }
                }
                var viewModel = new CarsCompanyViewModel();
                viewModel.CarsAndDrivers = new Dictionary<Car, DriverBasicInfo>();
                for (int i = 0; i < cars.Count; i++)
                {
                   
                        viewModel.CarsAndDrivers.Add(cars[i], listUsers[i]);
                }
                var driversWithoutCars = db.Drivers.Where(x => x.Car == null).ToList();
                var listSelectedItems = new List<SelectListItem>();
                listSelectedItems.Add(new SelectListItem { Selected = true, Text = string.Format("Няма"), Value = "0" });
                for (var  i= 0; i < driversWithoutCars.Count; i++)
                {
                    var user = UserManager.FindById(driversWithoutCars[i].UserId);
                    listSelectedItems.Add(new SelectListItem { Selected = false, Text = string.Format("{0} {1}", user.FirstName,user.LastName), Value = user.Id.ToString() });
                }
                viewModel.Drivers = new SelectList(listSelectedItems);

                return View("CompanyCars" , viewModel);

            }
            else
            {
                return View("DriverCars");
            }
        }

        [HttpPost]
        [Authorize(Roles ="Company")]
        [ValidateAntiForgeryToken]
         public ActionResult RegisterCar(CarsCompanyViewModel viewModel)
        {
            var userId = User.Identity.GetUserId();
            var company = db.Companies.Where(x => x.UserId == userId).First();
            var newcar = (new Car() { Brand = viewModel.Brand, InternalNumber = viewModel.InternalNumber, Model = viewModel.Model, RegisterNumber = viewModel.RegisterNumber, Year = viewModel.Year, Company = company, Location = new Location() { Latitude = 0, Longitude = 0 }, CarStatus = CarStatus.OffDuty });
            db.Cars.Add(newcar);
            db.SaveChanges();
            if (viewModel.SelectedDriver != "0")
            {
                var driver = db.Drivers.Where(x => x.UserId == viewModel.SelectedDriver).First();
                driver.Car = newcar;
                db.SaveChanges();
            }
            return RedirectToAction("Cars");
        }


        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeCar(CarsCompanyViewModel viewModel)
        {
            var car = db.Cars.Where(x => x.Id == viewModel.CarId).First();
            car.Model = viewModel.Model;
            car.RegisterNumber = viewModel.RegisterNumber;
            car.InternalNumber = viewModel.InternalNumber;
            car.Year = viewModel.Year;
            car.Brand = viewModel.Brand;
            db.SaveChanges();
            
            var driver = db.Drivers.Where(x => x.Car.Id == car.Id).FirstOrDefault();

            if (viewModel.SelectedDriver == "0" && driver != null)
            {
                driver.Car = null;
                
            }
            else if (viewModel.SelectedDriver != "0" && driver == null)
            {
                var newDriver = db.Drivers.Where(x => x.UserId == viewModel.SelectedDriver).First();
                newDriver.Car = car;
            }else if (viewModel.SelectedDriver != "0" && driver != null && viewModel.SelectedDriver != driver.UserId)
            { 
                driver.Car = null;
                var newDriver = db.Drivers.Where(x => x.UserId == viewModel.SelectedDriver).First();
                newDriver.Car = car;
            }

            db.SaveChanges();

            return RedirectToAction("Cars");
        }


        [HttpGet]
        [Authorize(Roles = "Company")]
        public ActionResult Drivers()
        {
            var userId = User.Identity.GetUserId();
            var company = db.Companies.Where(x => x.UserId ==  userId).First();
            var drivers = db.Drivers.Where(x => x.Company.Id == company.Id).ToList();
            var viewModel = new DriversCompanyViewModel() { Drivers = new List<BgTaxi.Models.Models.Driver>(), Users = new List<Models.ApplicationUser>() };

            foreach (var item in drivers)
            {
                viewModel.Drivers.Add(item);
               viewModel.Users.Add(UserManager.FindById(item.UserId));
            }
            return View("CompanyDrivers", viewModel);
        }
        [Authorize(Roles = "Company")]
        public ActionResult Reports()
        {
            return View();
        }

        [Authorize(Roles = "Driver")]
        public ActionResult Company()
        {
            return View();
        }
        [Authorize(Roles = "User")]
        public ActionResult Requests()
        {
            return View();
        }

        [HttpPost]

        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDriverFromCompany(int id)
        {
            var userid = User.Identity.GetUserId();
            var company = db.Companies.Where(x => x.UserId == userid).First();
            var driver = db.Drivers.Where(x => x.Id == id).First();
            if(driver.Company.Id == company.Id)
            {
                db.Drivers.Remove(driver);
            }
            db.SaveChanges();
            return RedirectToAction("Drivers");
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error,
            ProfileInfoUpdateSucess
        }

        #endregion
    }
}