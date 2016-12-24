using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BgTaxi.Models;
using System.Web.Http.Cors;
using BgTaxi.Web.ActionFilter;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;
using BgTaxi.Models.Models;
using System.Collections.Generic;
using System.Text;
using BgTaxi.Attributes;
using System.Data.Entity;
using System.Threading;

namespace BgTaxi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private Models.Models.Database db;

        public AccountController()
        {
            db = new Models.Models.Database();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // The Authorize Action is the end point which gets called when you access any
        // protected Web API. If the user is not logged in then they will be redirected to 
        // the Login page. After a successful login you can call a Web API.
        [HttpGet]
        public ActionResult Authorize()
        {
            var claims = new ClaimsPrincipal(User).Claims.ToArray();
            var identity = new ClaimsIdentity(claims, "Bearer");
            AuthenticationManager.SignIn(identity);
            return new EmptyResult();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [CaptchaValidator]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, bool captchaValid)
        {

            if (!ModelState.IsValid || !captchaValid)
            {
                return View(model);
            }

            var userid = UserManager.FindByEmail(model.Email).Id;
            if (!UserManager.IsEmailConfirmed(userid))
            {
                return View("EmailNotConfirmed");
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        [AllowCrossSiteJsonAttribute]
        [ThrottleAttribute(Seconds=180)]
        public JsonResult DeviceRegistration()
        {
            Device device = new Device() { LastRequestDateTime = DateTime.Now, UserId = null };
            AccessToken accToken = new AccessToken()
            {
                Device = device,
                UniqueAccesToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                CreatedDateTime = DateTime.Now
            };

            db.Devices.Add(device);
            db.AccessTokens.Add(accToken);
            db.SaveChanges();
            return Json(new { accessToken = accToken.UniqueAccesToken });

        }

        [AllowAnonymous]
        [AllowCrossSiteJsonAttribute]
        public JsonResult LoginExternal(string basicAuth, string requiredRoleId, string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

                var userIdObj = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Select(y => new { userId = y.Device.UserId }).FirstOrDefault();
                if (userIdObj != null)
                {
                    if (userIdObj.userId != null)
                    {
                        return Json(new { status = "USER LOGGED IN", accessToken = newAccessToken });
                    }
                }
                else
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }
                // This doesn't count login failures towards account lockout
                //// To enable password failures to trigger account lockout, change to shouldLockout: true
                //var result = await SignInManager.PasswordSignInAsync(email, password, false, shouldLockout: false);
                 var usernamePass = ExtractUserNameAndPassword(basicAuth);
                var findAcync = UserManager.FindAsync(usernamePass.Item1, usernamePass.Item2);

                if (findAcync.Result != null)
                {
                    var roleRequired = findAcync.Result.Roles.Any(x => x.RoleId == requiredRoleId);
                    if (roleRequired && findAcync.Result.EmailConfirmed)
                    {
                        switch (requiredRoleId)
                        {
                            case "1":
                                return Json(new { status = "NO PERMISSION", accessToken = newAccessToken });

                            case "2":
                                bool haveCar = db.Drivers.Any(x => x.Car != null && x.UserId == findAcync.Result.Id);
                                bool haveCompany = db.Drivers.Any(x => x.Company != null && x.UserId == findAcync.Result.Id);
                                bool alreadyLoggedIn = db.Devices.Any(x => x.UserId == findAcync.Result.Id);
                                if (haveCar && haveCompany && !alreadyLoggedIn)
                                {
                                    var accTok = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(x => x.Device).First();
                                    accTok.Device.UserId = findAcync.Result.Id;
                                    accTok.Device.LastRequestDateTime = DateTime.Now;
                                    db.SaveChanges();
                                        return Json(new { status = "OK", accessToken = newAccessToken });

                                } else if (alreadyLoggedIn)
                                {
                                    return Json(new { status = "ALREADY LOGGED IN", accessToken = newAccessToken });
                                }
                                else if (!haveCompany)
                                {
                                    return Json(new { status = "NO COMPANY", accessToken = newAccessToken });
                                }
                                else if (!haveCar)
                                {
                                    return Json(new { status = "NO CAR", accessToken = newAccessToken });
                                }
                                
                                break;
                            case "3":

                                return Json(new { status = "OK", accessToken = newAccessToken });
                            default:
                                return Json(new { status = "ERR", accessToken = newAccessToken });
                        }

                    }else if (!roleRequired)
                    {
                        return Json(new { status = "ROLE NOT MATCH", accessToken = newAccessToken });
                    }
                    else
                    {
                        return Json(new { status = "NO EMAIL", accessToken = newAccessToken });
                    }
                    
                }

                return Json(new { status = "ERR", accessToken = newAccessToken });

            }
            else
            {
                return Json(new { status = "CT" });
            }
        }
        [AllowAnonymous]
        [AllowCrossSiteJsonAttribute]
        public JsonResult LogoutExternal(string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);
                if(newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }

               var accTok =  db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(x => x.Device).First();
                if(accTok.Device.UserId != null)
                {
                    accTok.Device.UserId = null;
                    accTok.Device.LastRequestDateTime = DateTime.Now;
                    db.SaveChanges();
                    return Json(new { status = "OK", accessToken = newAccessToken });
                }
                else
                {
                    return Json(new { status = "NO USER", accessToken = newAccessToken });
                }
            
            }
            else
            {
                return Json(new { status = "CT" });
            }
        }



        public async Task<bool> AccountValidation(string password, string username)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(username, password, false, shouldLockout: false);

            if (result == SignInStatus.Success) {
                return true;

            }
            return false;
            
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult RegisterClient()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult RegisterCompany()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult RegisterDriver()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterClient(RegisterClientViewModel model)
        {
            if (ModelState.IsValid)
            {
               
                    var user = new Models.ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, FirstName = model.FirstName, LastName = model.LastName };
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        UserManager.AddToRole(user.Id, "User");
                        //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Потвърдете вашата регистрация", string.Format("<p><span style='font-family:times new roman,times,serif;'>Здравейте {0},<br/>Вие успешно регистрирахте в сайта bgtaxi.com.</span></p><p>Моля, активирайте вашия акаунт, като натиснете върху линка по-долу:</p><h2><a href='{1}'>Активирай сега</a></h2>", user.FirstName, callbackUrl));

                        return View("SuccessfulRegistration");
                    }
                    AddErrors(result);
                }
            

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterDriver(RegisterDriverViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Companies.Any(x => x.UniqueNumber == model.UniqueNumber))
                {
                   
                    
                    var user = new Models.ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, FirstName= model.FirstName, LastName = model.LastName };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    
                    if (result.Succeeded)
                    {
                        var company = db.Companies.Where(x => x.UniqueNumber == model.UniqueNumber).First();
                        UserManager.AddToRole(user.Id, "Driver");
                        db.Drivers.Add(new BgTaxi.Models.Models.Driver { UserId = user.Id, Company = company });
                        db.SaveChanges();
                        //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Потвърдете вашата регистрация", string.Format("<p><span style='font-family:times new roman,times,serif;'>Здравейте {0},<br/>Вие успешно регистрирахте в сайта bgtaxi.com.</span></p><p>Моля, активирайте вашия акаунт, като натиснете върху линка по-долу:</p><h2><a href='{1}'>Активирай сега</a></h2>", user.FirstName, callbackUrl));

                        return View("SuccessfulRegistration");
                    }
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterCompany(RegisterCompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                const string AllowedChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                Random rng = new Random();
                var uniqueString = RandomString(AllowedChars, 6, rng);
                while((db.Companies.Any(x => x.UniqueNumber == uniqueString))){
                    uniqueString = RandomString(AllowedChars, 6, rng);
                }                

                var user = new Models.ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, FirstName= model.FirstName, LastName = model.LastName };

                var result = await UserManager.CreateAsync(user, model.Password);
              
                if (result.Succeeded)
                {
                    var company = new Company { Name = model.CompanyName, Address = model.Address, DDS = model.DDS, EIK = model.EIK, MOL = model.MOL, UserId = user.Id, UniqueNumber = uniqueString };
                    db.Companies.Add(company);
                    db.SaveChanges();
                    UserManager.AddToRole(user.Id, "Company");

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    //Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    string email = String.Format("<p><span style='font-family:times new roman,times,serif;'>Здравейте {0},<br/>Вие успешно регистрирахте Вашата фирма:<br/>{1}&nbsp;<br/>ЕИК:{2}<br/>ДДС №:{3}<br/>Адрес №:{4}<br/>МОЛ:{5}</span></p><h3><a href='{7}'> Активирайте вашия акаунт!</a></h3><h3><span style='font-family:times new roman,times,serif;'>Уникалният код на Вашата&nbsp;фирмата е:</span></h3><h2 style='text-align: center;'><span style='font-family:times new roman,times,serif;'><span><b>{6}</b></span></span></h2><h3 style='text-align: center;'><span style='font-family:times new roman,times,serif;'><span>Този код е изключетелно важен, защото чрез него вашите служители ще помогат да направят своите регистрации в сайта bgtaxi.net</span></span></h3>",
                        user.FirstName, company.Name, company.EIK, company.DDS, company.Address, company.MOL, uniqueString, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, "Регистрация в bgtaxi.net", email);

                    return View("SuccessfulRegistration");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private static string RandomString(string allowedChars, int stringLength , Random rd)
        {
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        //[AllowAnonymous]
        //public JsonResult CreateRole(string roleName)
        //{

        //    var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
        //    var roleresult = RoleManager.Create(new IdentityRole(roleName));

        //    return Json("OK");
        //}

        //TODO: Some security
        [AllowAnonymous]
        [AllowCrossSiteJson]
        public async Task<JsonResult> RegisterExternal(string email, string password, string dataRole)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                if (ModelState.IsValid)
                {
                    var user = new Models.ApplicationUser { UserName = email, Email = email };
                    var result = await UserManager.CreateAsync(user, password);
                    UserManager.AddToRole(user.Id, dataRole);
                    if (result.Succeeded)
                    {
                        return Json(new { status = "OK" });
                    }
                    AddErrors(result);
                }

                // If we got this far, something failed, redisplay form
                return Json(new { status = "ERR" });
            }
            else
            {
                return Json(new { status = "CT" });
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new Models.ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
        private static Tuple<string, string> ExtractUserNameAndPassword(string authorizationParameter)
        {
            byte[] credentialBytes;

            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return null;
            }

            // The currently approved HTTP 1.1 specification says characters here are ISO-8859-1.
            // However, the current draft updated specification for HTTP 1.1 indicates this encoding is infrequently
            // used in practice and defines behavior only for ASCII.
            Encoding encoding = Encoding.ASCII;
            // Make a writable copy of the encoding to enable setting a decoder fallback.
            encoding = (Encoding)encoding.Clone();
            // Fail on invalid bytes rather than silently replacing and continuing.
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
            string decodedCredentials;

            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return null;
            }

            if (String.IsNullOrEmpty(decodedCredentials))
            {
                return null;
            }

            int colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
            {
                return null;
            }

            string userName = decodedCredentials.Substring(0, colonIndex);
            string password = decodedCredentials.Substring(colonIndex + 1);
            return new Tuple<string, string>(userName, password);
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
          
        }
        #endregion
    }

    internal class captchaValidatorAttribute : Attribute
    {
    }
}
