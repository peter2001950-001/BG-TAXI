using BgTaxi.Controllers;
using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Web.ActionFilter;
using BgTaxi.Web.GoogleRequests;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace BgTaxi.Web.Controllers
{
    [AllowCrossSiteJsonAttribute]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RequestController : Controller
    {
        Database db = new Database();


        // GET: Request

        //[Throttle(Name = "TestThrottle", Message = "You must wait {n} seconds before accessing this url again.", Seconds = 20)]

        //public JsonResult CreateRequest(double lon, double lat)
        //{
        //    string username = DateTime.Now.ToString();
        //    if (db.ActiveRequests.Any(x => x.Username == username))
        //    {
        //        return Json(new { message = "There is an active request from this user", status = "ERR" });
        //    }

        //  //var elementMax =  db.Keys.Where(y => y.Type == "request").FirstOrDefault();
        //    var newItem = new ActiveRequests();

        //    newItem.Location = new Location() { Latitude = lat, Longitude = lon };
        //        newItem.Username = username;
        //       newItem.DateTimeCreated = DateTime.Now;
        //        //newItem.Id = ++elementMax.LastValue;

        //    db.ActiveRequests.Add(newItem);
        //    //elementMax.LastValue++;
        //    db.SaveChanges();
        //    var request = db.ActiveRequests.Where(x => x.Username == username)
        //        .FirstOrDefault();

        //    return Json(new { message = "The request was send for successfully", status = "OK", requestId = request.Id }, JsonRequestBehavior.AllowGet);
        //}

            public JsonResult AddCompanyCode(string basicAuth, string companyCode)
             {
            if (HttpContext.Request.RequestType == "POST")
            {
                var usernamePass = ExtractUserNameAndPassword(basicAuth);
                var user = UserGet(usernamePass.Item1, usernamePass.Item2);
                if (user != null)
                {
                    if (db.Companies.Any(x => x.UniqueNumber == companyCode))
                    {
                        var company = db.Companies.Where(x => x.UniqueNumber == companyCode).First();
                        db.Drivers.Add(new Driver { Company = company, UserId = user.Id});
                        db.SaveChanges();

                        return Json(new { status = "OK" });
                    }
                    else
                    {
                        return Json(new { status = "WRONG CODE" });
                    }
                }
                else
                {
                    return Json(new { status = "NO AUTH" });
                }
            }
            else
            {
                return Json(new { });
            }
        }
        public JsonResult RequestStatus(int requestID, string basicAuth)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var usernamePass = ExtractUserNameAndPassword(basicAuth);
                var user = UserGet(usernamePass.Item1, usernamePass.Item2);
                if (user == null)
                {
                    return Json(new { message = "There is not any user with this authString", status = "ERR" });
                }
                if (db.ActiveRequests.Any(x => x.Id == requestID))
                {
                    return Json(new { status = "NOT TAKEN" });
                }
                else
                {
                    if (db.TakenRequests.Where(x => x.RequestId == requestID).FirstOrDefault().UserInformed)
                    {
                       
                        return Json(new { status = "NO CHANGE" });
                    }
                    else
                    {
                        var requestData = db.TakenRequests.Where(x => x.RequestId == requestID).Select(x => new { DriverLocation = x.DriverLocation, UserLocation = x.UserLocation, CompanyId = x.Company.Id, CarId = x.Car.Id, OnAddress = x.OnAddress }).FirstOrDefault();
                        if (requestData != null)
                        {
                            if (requestData.OnAddress == false)
                            {
                                var duration = GoogleAPIRequest.GetDistance(requestData.DriverLocation, requestData.UserLocation);
                                var company = db.Companies.Where(x => x.Id == requestData.CompanyId).FirstOrDefault();
                                var car = db.Cars.Where(x => x.Id == requestData.CarId).FirstOrDefault();
                                db.TakenRequests.Where(x => x.RequestId == requestID).FirstOrDefault().UserInformed = true;
                                db.SaveChanges();
                                return Json(new { status = "TAKEN", distance = duration.rows[0].elements[0].distance.text, duration = duration.rows[0].elements[0].duration.text, carRegNum = car.RegisterNumber, companyName = company.Name });
                            }
                            else
                            {

                                db.TakenRequests.Where(x => x.RequestId == requestID).FirstOrDefault().UserInformed = true;
                                db.SaveChanges();
                                return Json(new { status = "ON ADDRESS" });
                            }

                        }
                        return Json(new { status = "NO REQUEST" });
                    }
                }
            }
            else
            {
                return Json(new { });
            }
        }

        
        public JsonResult TakeRequest(int requestID, string basicAuth, double lon, double lat)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                if (db.ActiveRequests.Any(x => x.Id == requestID))
                {
                    var usernamePass = ExtractUserNameAndPassword(basicAuth);
                    var user = UserGet(usernamePass.Item1, usernamePass.Item2);

                    var driver = db.Drivers.Where(y => y.UserId == user.Id).Select(x=> new  { CarId = x.Car.Id, }).FirstOrDefault();
                    if (driver != null)
                    {
                        var car = db.Cars.Where(x => x.Id == driver.CarId).Select(x=> new {TheObject = x, companyId = x.Company.Id  }).FirstOrDefault();
                        var company = db.Companies.Where(x=>x.Id == car.companyId).FirstOrDefault();
                        var request = db.ActiveRequests.Where(x => x.Id == requestID).FirstOrDefault();
                    db.TakenRequests.Add(new TakenRequests()
                    {
                        RequestId = requestID,
                        DateTimeCreated = request.DateTimeCreated,
                        UserLocation = request.Location,
                        ClientUserId = request.UserId,
                        DateTimeTaken = DateTime.Now,
                        Car = car.TheObject,
                        Company = company,
                        DriverLocation = new Models.Models.Location() {  Latitude = lat, Longitude = lon},
                        DriverUserId = user.Id
                    });
                   
                    db.ActiveRequests.Remove(request);
                    db.SaveChanges();

                    // move the request to the RequestHistory
                    return Json(new { status = "OK" });

                    }
                    else
                    {
                        return Json(new { status = "ERR" });
                    }
                }
                else
                {
                    return Json(new { status = "REMOVED" });
                }
            }
            else
            {
                return Json(new { });
            }
        }

        public JsonResult UpdateStatus(double lat, double lon, string basicAuth, int requestId, bool onAddress = false)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                if(db.TakenRequests.Any(x=> x.RequestId == requestId))
                {
                    var usernamePass = ExtractUserNameAndPassword(basicAuth);
                    var user = UserGet(usernamePass.Item1, usernamePass.Item2);

                    var request = db.TakenRequests.Where(x => x.RequestId == requestId).FirstOrDefault();
                    if(request.DriverUserId == user.Id)
                    {
                        if (onAddress == false)
                        {
                            request.DriverLocation = new Models.Models.Location() { Latitude = lat, Longitude = lon };
                        }
                        else
                        {
                            if (!request.OnAddress)
                            {
                                request.OnAddress = true;
                                request.UserInformed = false;
                            }
                        }
                        db.SaveChanges();

                        if (request.UserInformed)
                        {
                            return Json(new { status = "OK", clientInformed = true });
                        }
                        else
                        {
                            return Json(new { status = "OK", clientInformed = false });
                        }
                    }
                }
                return Json(new { status = "ERR" });
            }
            else
            {
                return Json(new { });
            }
        }
        public JsonResult FinishRequest(int requestId, string basicAuth)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var usernamePass = ExtractUserNameAndPassword(basicAuth);
                var user = UserGet(usernamePass.Item1, usernamePass.Item2);
                  var request = db.TakenRequests.Where(x => x.RequestId == requestId).FirstOrDefault();
                if (request != null) {
                    if (user.Id == request.DriverUserId)
                    {
                        db.RequestHistory.Add(new RequestHistory()
                        {
                            RequestId = request.RequestId,
                            DriverUserId = request.DriverUserId,
                            Car = request.Car,
                            ClientUserId = request.ClientUserId,
                            Company = request.Company,
                            DateTimeCreated = request.DateTimeCreated,
                            DateTimeTaken = request.DateTimeTaken

                        });
                        db.TakenRequests.Remove(request);
                        db.SaveChanges();

                        return Json(new { status = "OK" });
                    }
                }
                return Json(new { status = "ERR" });
            }
            else
            {
                return Json(new { });
            }

        }
        public JsonResult GetAppropriateRequest(double lat, double lon, string basicAuth)
        {
            //TODO: basicAuth
            if (HttpContext.Request.RequestType == "POST")
            {
                var usernamePass = ExtractUserNameAndPassword(basicAuth);
                var user = UserGet(usernamePass.Item1, usernamePass.Item2);
                if (user.Roles.Any(x => x.RoleId == "2"))
                {
                    bool haveCar = db.Drivers.Any(x => x.Car != null && x.UserId == user.Id);
                    bool haveCompany = db.Drivers.Any(x => x.Company != null && x.UserId == user.Id);
                    if (haveCar && haveCar)
                    {
                        var nearByRequests = db.ActiveRequests.Where(x => Math.Abs(x.Location.Latitude - lat) <= 0.0165 && Math.Abs(x.Location.Longitude - lon) <= 0.0165);

                        List<object> requestList = new List<object>();
                        foreach (var item in nearByRequests)
                        {
                            var distance = Math.Round(DistanceBetweenTwoPoints.GetDistance(item.Location, new Models.Models.Location() { Latitude = lat, Longitude = lon }), 3);
                            if (distance < 1.5)
                            {
                                var address = GoogleAPIRequest.GetAddress(item.Location.Latitude, item.Location.Longitude);
                                requestList.Add(new { distance = distance, id = item.Id, address = address });
                            }
                        }
                        return Json(new { requests = requestList, status = "OK" });
                    }
                }
                return Json(new { stutus = "NO PERMISSION" });
            }
            else
            {
                return Json(new { });
            }
        }

        public JsonResult GetInfoForRequest(int requestId, double lon, double lat, string basicAuth)
        {
            // TODO: basicAuth
            var request = db.ActiveRequests.Where(a => a.Id == requestId).FirstOrDefault();
            var duration = GoogleAPIRequest.GetDistance(request.Location, new Models.Models.Location() { Latitude = lat, Longitude = lon });
            return Json(new { lon = request.Location.Longitude, lat = request.Location.Latitude, distance = duration.rows[0].elements[0].distance.text, duration = duration.rows[0].elements[0].duration.text });
        }

        public JsonResult CreateNewRequest(double lon, double lat, string basicAuth)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                //var user = User.Identity.Name;

                var address = GoogleAPIRequest.GetAddress(lat, lon);
                var usernamePass = ExtractUserNameAndPassword(basicAuth);
                var user = UserGet(usernamePass.Item1, usernamePass.Item2);
                if (user == null)
                {
                    return Json(new { message = "There is not any user with this authString", status = "ERR" });
                }
                if (db.ActiveRequests.Any(x => x.UserId == user.Id))
                {
                    var record = db.ActiveRequests.Where(x => x.UserId == user.Id).SingleOrDefault();
                    return Json(new { message = "The request was send for successfully", status = "OK", requestId = record.Id, address = address });
                }

                var newItem = new ActiveRequests();

                newItem.Location = new Models.Models.Location() { Latitude = lat, Longitude = lon };
                newItem.UserId = user.Id;
                newItem.DateTimeCreated = DateTime.Now;

                db.ActiveRequests.Add(newItem);
                db.SaveChanges();
                var id = newItem.Id;
                
                return Json(new { message = "The request was send for successfully", status = "OK", requestId = id, address = address });
            }
            else
            {
                return Json(new { });
            }

        }

        private static Models.ApplicationUser UserGet(string userName, string password)
        {

            UserManager<Models.ApplicationUser> userManager = new UserManager<Models.ApplicationUser>(new UserStore<Models.ApplicationUser>(new Models.ApplicationDbContext()));
            var user = userManager.FindAsync(userName, password);
            if (user == null)
            {
                return null;
            }
            return user.Result;
        }
        private static UserManager<IdentityUser> CreateUserManager()
        {
            return new UserManager<IdentityUser>(new UserStore<IdentityUser>(new Models.ApplicationDbContext()));
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

    }
}
