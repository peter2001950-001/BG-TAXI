﻿using System;
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
using BgTaxi.Services.Contracts;

namespace BgTaxi.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private readonly ICompanyService _companyService;
        private readonly ICarService _carService;
        private readonly IDriverService _driverService;
        private readonly IRequestService _requestService;
        private readonly IDispatcherService _dispatcherService;

        public ManageController(ICompanyService companyService, ICarService carService, IDriverService driverService, IDispatcherService dispatcherService, IRequestService requestService)
        {
            this._companyService = companyService;
            this._carService = carService;
            this._driverService = driverService;
            this._dispatcherService = dispatcherService;
            this._requestService = requestService;
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

            if (User.IsInRole("Company"))
            {

                var company = _companyService.GetAll().First(x => x.UserId == userId);
                var viewModel = new IndexCompanyViewModel { UniqueNumber = company.UniqueNumber, Name = company.Name, MOL = company.MOL, Address = company.City, DDS = company.DDS, EIK = company.EIK };

                return View("IndexCompany", viewModel);
            }
            else if (User.IsInRole("Driver"))
            {
                return View("IndexDriver", model);

            }
            else if (User.IsInRole("Dispatcher"))
            {
                UserManager<Models.ApplicationUser> userManager = new UserManager<Models.ApplicationUser>(new UserStore<Models.ApplicationUser>(new Models.ApplicationDbContext()));
                var user = userManager.FindById(User.Identity.GetUserId());
                var viewModel = new IndexDispatcherViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Telephone = user.PhoneNumber
                };
                var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == user.Id);
                var company = _companyService.GetAll().First(x => x.Id == dispatcher.Company.Id);
                viewModel.CompanyName = company.Name;

                return View("IndexDispatcher", viewModel);
            }
            else
            {
                return View("IndexDriver", model);
            }

        }
        /// <summary>
        /// Update company information
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCompanyInfo(IndexCompanyViewModel viewModel)
        {
            var company = _companyService.UpdateCompany(viewModel.UniqueNumber, viewModel.Name, viewModel.MOL, viewModel.EIK, viewModel.DDS, viewModel.Address);
            var newViewModel = new IndexCompanyViewModel() { Address = company.City, DDS = company.DDS, EIK = company.EIK, MOL = company.MOL, UniqueNumber = company.UniqueNumber, Name = company.Name };


            return RedirectToAction("Index", new { message = ManageMessageId.ProfileInfoUpdateSucess });
        }
        /// <summary>
        /// Update dispatcher's account information
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
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


            var newViewModel = new IndexDispatcherViewModel() { FirstName = user.FirstName, Telephone = user.Telephone, LastName = user.LastName, CompanyName = viewModel.CompanyName };


            return RedirectToAction("Index", new { message = ManageMessageId.ProfileInfoUpdateSucess });
        }


        /// <summary>
        /// Cars Page for the manager
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Company, Driver")]
        public ActionResult Cars()
        {


            if (User.IsInRole("Company"))
            {

                var currentUserId = User.Identity.GetUserId();
                var cars = _carService.GetCars().Where(x => x.Company.UserId == currentUserId).ToList();
                var drivers = new List<Driver>();
                foreach (var car in cars)
                {
                    var driver = _driverService.GetDriverByCar(car);
                    if (driver != null)
                    {
                        drivers.Add(driver);
                    }
                    else
                    {
                        drivers.Add(null);
                    }
                }
                var listUsers = new List<DriverBasicInfo>();
                foreach (var driver in drivers)
                {
                    if (driver != null)
                    {
                        var userFound = UserManager.FindById(driver.UserId);
                        listUsers.Add(new DriverBasicInfo() { UserId = userFound.Id, Name = userFound.FirstName + ' ' + userFound.LastName });
                    }
                    else
                    {
                        listUsers.Add(new DriverBasicInfo() { Name = "Няма", UserId = "0" });
                    }
                }
                var viewModel = new CarsCompanyViewModel { CarsAndDrivers = new Dictionary<Car, DriverBasicInfo>() };
                for (var i = 0; i < cars.Count; i++)
                {
                    if (listUsers.Count - 1 < i)
                    {
                        viewModel.CarsAndDrivers.Add(cars[i], new DriverBasicInfo() { Name = "Няма", UserId = "0" });
                    }
                    else
                    {
                        viewModel.CarsAndDrivers.Add(cars[i], listUsers[i]);
                    }
                }
                var driversWithoutCars = _driverService.GetAll().Where(x => x.Car == null).ToList();
                var listSelectedItems = new List<SelectListItem>();
                listSelectedItems.Add(new SelectListItem { Selected = true, Text = string.Format("Няма"), Value = "0" });
                for (var i = 0; i < driversWithoutCars.Count; i++)
                {
                    var user = UserManager.FindById(driversWithoutCars[i].UserId);
                    listSelectedItems.Add(new SelectListItem
                    {
                        Selected = false,
                        Text =
                       user.FirstName + ' ' + user.LastName,
                        Value = user.Id.ToString()
                    });
                }
                viewModel.Drivers = new SelectList(listSelectedItems);

                return View("CompanyCars", viewModel);

            }
            else
            {
                return View("DriverCars");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Company")]
        public ActionResult Dispatchers()
        {

            var userId = User.Identity.GetUserId();
            var company = _companyService.GetAll().First(x => x.UserId == userId);
            var dispatcher = _dispatcherService.GetAll().Where(x => x.Company.Id == company.Id).ToList();
            var viewModel = new DispatcherCompanyViewModel() { Dispatchers = new List<BgTaxi.Models.Models.Dispatcher>(), Users = new List<Models.ApplicationUser>() };

            foreach (var item in dispatcher)
            {
                viewModel.Dispatchers.Add(item);
                viewModel.Users.Add(UserManager.FindById(item.UserId));
            }
            return View("CompanyDispatchers", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterCar(CarsCompanyViewModel viewModel)
        {
            var userId = User.Identity.GetUserId();
            var company = _companyService.GetAll().First(x => x.UserId == userId);
            var newcar = (new Car() { Brand = viewModel.Brand, InternalNumber = viewModel.InternalNumber, Model = viewModel.Model, RegisterNumber = viewModel.RegisterNumber, Year = viewModel.Year, Company = company, Location = new Location() { Latitude = 0, Longitude = 0 }, CarStatus = CarStatus.OffDuty, LastActiveDateTime = DateTime.Now });
            _carService.CreateCar(newcar);
            if (viewModel.SelectedDriver != "0")
            {
                var driver = _driverService.GetAll().First(x => x.UserId == viewModel.SelectedDriver);
                _driverService.AddCar(driver.Id, newcar);
            }
            return RedirectToAction("Cars");
        }

        /// <summary>
        /// Change car information 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeCar(CarsCompanyViewModel viewModel)
        {

            var car = _carService.GetCars().First(x => x.Id == viewModel.CarId);

            car.Model = viewModel.Model;
            car.RegisterNumber = viewModel.Model;
            car.InternalNumber = viewModel.InternalNumber;
            car.Year = viewModel.Year;
            car.Brand = viewModel.Brand;

            _carService.ModifyCar(viewModel.CarId, car);
            var driver = _driverService.GetDriverByCar(car);
            if (viewModel.SelectedDriver == "0" && driver != null)
            {
                driver.Car = null;
                _driverService.DriverModify(driver.Id, driver);

            }
            else if (viewModel.SelectedDriver != "0" && driver == null)
            {
                var newDriver = _driverService.GetAll().First(x => x.UserId == viewModel.SelectedDriver);
                _driverService.AddCar(newDriver.Id, car);
            }
            else if (viewModel.SelectedDriver != "0" && driver != null && viewModel.SelectedDriver != driver.UserId)
            {
                driver.Car = null;
                _driverService.DriverModify(driver.Id, driver);
                var newDriver = _driverService.GetAll().FirstOrDefault(x => x.UserId == viewModel.SelectedDriver);
                if(newDriver!=null)
                _driverService.AddCar(newDriver.Id, car);
            }
            
            return RedirectToAction("Cars");
        }


        [HttpGet]
        [Authorize(Roles = "Company")]
        public ActionResult Drivers()
        {
            var userId = User.Identity.GetUserId();
            var company = _companyService.GetAll().First(x => x.UserId == userId);
            var drivers = _driverService.GetAll().Where(x => x.Company.Id == company.Id).ToList();
            var viewModel = new DriverCompanyViewModel() { Drivers = new List<BgTaxi.Models.Models.Driver>(), Users = new List<Models.ApplicationUser>() };

            foreach (var item in drivers)
            {
                viewModel.Drivers.Add(item);
                viewModel.Users.Add(UserManager.FindById(item.UserId));
            }
            return View($"CompanyDrivers", viewModel);
        }

        /// <summary>
        /// Returns JSON data needed for the charts to be drawn
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        [Authorize(Roles = "Company")]
        public JsonResult GetReport(string month)
        {
            var monthTaken = month.Split('-');
            int yearInt = int.Parse(monthTaken[0]);
            int monthInt = int.Parse(monthTaken[1]);

            var days = DateTime.DaysInMonth(yearInt, monthInt);
            var startingDate = new DateTime(yearInt, monthInt, 1).AddDays(-1);
            var endingDate = new DateTime(yearInt, monthInt, days).AddDays(1);

            int[] acceptedRequestsByDateResults = new int[days];
            int[] carsByDateResults = new int[days];
            var acceptedRequests = _requestService.GetRequestHistories()
                .Where(x => x.Request.CreatedDateTime > startingDate && x.Request.CreatedDateTime < endingDate)
                .ToList();
            List<string> carsCountedId = new List<string>();
            int dayCurrent = 1;
            foreach (var item in acceptedRequests)
            {

                var currentDay = item.Request.CreatedDateTime.Day;
                acceptedRequestsByDateResults[currentDay - 1]++;

                if (currentDay != dayCurrent)
                {
                    dayCurrent = currentDay;
                    carsCountedId.Clear();
                }
                if (!carsCountedId.Any(x => x == item.Car.Id.ToString()))
                {
                    carsCountedId.Add(item.Car.Id.ToString());
                    carsByDateResults[currentDay - 1]++;
                }
            }

            var allRequests = _requestService.GetRequestInfos()
                .Where(x => x.CreatedDateTime > startingDate && x.CreatedDateTime < endingDate)
                .ToList();

            var allRequestsByDateResults = new int[days];
            foreach (var item in allRequests)
            {
                var currentDay = item.CreatedDateTime.Day;
                allRequestsByDateResults[currentDay - 1]++;
            }

            object[] allRequestsByDateObject = new object[days];
            for (int i = 0; i < allRequestsByDateResults.Length; i++)
            {
                allRequestsByDateObject[i] = new { day = i + 1, value = allRequestsByDateResults[i] };
            }
            object[] acceptedRequestsByDateObject = new object[days];
            for (int i = 0; i < acceptedRequestsByDateResults.Length; i++)
            {
                acceptedRequestsByDateObject[i] = new { day = i + 1, value = acceptedRequestsByDateResults[i] };
            }
            object[] carsByDateObject = new object[days];
            for (int i = 0; i < carsByDateResults.Length; i++)
            {
                carsByDateObject[i] = new { day = i + 1, value = carsByDateResults[i] };
            }


            return Json(new { status = "OK", acceptedRequestsByDate = acceptedRequestsByDateObject, allRequestsByDate = allRequestsByDateObject, carsByDate = carsByDateObject });
        }


        [Authorize(Roles = "Company")]
        public ActionResult Reports()
        {
            return View($"CompanyReports");
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
        /// <summary>
        /// Remove Driver 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDriverFromCompany(int id)
        {
            var userid = User.Identity.GetUserId();
            var company = _companyService.GetAll().First(x => x.UserId == userid);
            var driver = _driverService.GetAll().First(x => x.Id == id);
            if (driver.Company.Id == company.Id)
            {
                _driverService.RemoveDriver(driver);
            }
            return RedirectToAction($"Drivers");
        }
        /// <summary>
        /// Remove Dispatcher
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDispatcherFromCompany(int id)
        {
            var userid = User.Identity.GetUserId();
            var company = _companyService.GetAll().First(x => x.UserId == userid);
            var dispatcher = _dispatcherService.GetAll().First(x => x.Id == id);
            if (dispatcher.Company.Id == company.Id)
            {
                _dispatcherService.RemoveDispatcher(dispatcher);
            }
            return RedirectToAction($"Dispatchers");
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
            return RedirectToAction($"ManageLogins", new { Message = message });
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