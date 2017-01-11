using BgTaxi.Controllers;
using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Web.ActionFilter;
using BgTaxi.Web.GoogleRequests;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        Models.Models.Database db = new Models.Models.Database();


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
        public JsonResult RequestStatus(int requestID, string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

                if(newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }
                var user = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(y => y.Device).First().Device.UserId;
                if (user == null)
                {
                    return Json(new { message = "NO USER", accessToken = newAccessToken });
                }

                if (db.ActiveRequests.Any(x => x.Id == requestID))
                {
                    return Json(new { status = "NOT TAKEN", accessToken = newAccessToken });
                }
                else
                {
                    if (db.TakenRequests.Where(x => x.Request.Id == requestID).FirstOrDefault().UserInformed)
                    {
                       
                        return Json(new { status = "NO CHANGE", accessToken = newAccessToken });
                    }
                    else
                    {
                        var requestData = db.TakenRequests.Where(x => x.Request.Id == requestID).Include(x=>x.Car).Include(x=>x.Company).Include(x=>x.Request).FirstOrDefault();
                        if (requestData != null)
                        {
                            if (requestData.OnAddress == false)
                            {
                                var driverLocation = db.Cars.Where(x => x.Id == requestData.Car.Id).First().Location;
                                var duration = GoogleAPIRequest.GetDistance(driverLocation, requestData.Request.StartingLocation);
                              
                                db.TakenRequests.Where(x => x.Request.Id == requestID).FirstOrDefault().UserInformed = true;
                                db.SaveChanges();
                                return Json(new { status = "TAKEN", distance = duration.rows[0].elements[0].distance.text, duration = duration.rows[0].elements[0].duration.text, carRegNum = requestData.Car.RegisterNumber, companyName = requestData.Company.Name, accessToken = newAccessToken });
                            }
                            else
                            {

                                db.TakenRequests.Where(x => x.Request.Id == requestID).FirstOrDefault().UserInformed = true;
                                db.SaveChanges();
                                return Json(new { status = "ON ADDRESS", accessToken = newAccessToken });
                            }

                        }
                        return Json(new { status = "NO REQUEST", accessToken = newAccessToken });
                    }
                }
            }
            else
            {
                return Json(new { });
            }
        }

        public JsonResult Pull(double lon, double lat, string accessToken, bool absent = false, bool free = false)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }

                var userId = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(y => y.Device).First().Device.UserId;

                var driver = db.Drivers.Where(x => x.UserId == userId).Include(x => x.Car).First();
                driver.Car.Location.Latitude = lat;
                driver.Car.Location.Longitude = lon;

                
                if (absent)
                {
                    driver.Car.CarStatus = CarStatus.Absent;
                }
                if(free)
                {
                    driver.Car.CarStatus = CarStatus.Free;
                }

                db.SaveChanges();
                if (db.ActiveRequests.Any(x => x.AppropriateCar.Id == driver.Car.Id))
                {
                    var request = db.ActiveRequests.Where(x => x.AppropriateCar.Id == driver.Car.Id).Include(x=>x.Request).First();
                    var distance = Math.Round(DistanceBetweenTwoPoints.GetDistance(request.Request.StartingLocation, new Models.Models.Location() { Latitude = lat, Longitude = lon }), 3);
                    object requestObj = new {distance = distance, address= request.Request.StartingAddress, id = request.Request.Id };
                    return Json(new { status = "OK", accessToken = newAccessToken, request = requestObj });
                }

                return Json(new { status = "OK", accessToken = newAccessToken });
            }
            else
            {
                return Json(new { });
            }

        }

        
        public JsonResult RequestAnswer(int requestID, string accessToken, bool answer)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

                    if (newAccessToken == null)
                    {
                        return Json(new { status = "INVALID ACCESSTOKEN" });
                    }
                if (db.ActiveRequests.Any(x => x.Request.Id == requestID))
                {
                    
                    var user = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(y => y.Device).First().Device.UserId;
                    var driver = db.Drivers.Where(y => y.UserId == user).Include(x => x.Car).Include(x => x.Company).FirstOrDefault();
                    if (!answer)
                    {
                        var requestSelected = db.RequestsInfo.Where(x => x.Id == requestID).First();
                        requestSelected.RequestStatus = RequestStatusEnum.NoCarChosen;
                        db.CarsDismissedRequests.Add(new CarDismissedRequest() { Car = driver.Car, Request = requestSelected });
                        db.SaveChanges();
                        return Json(new { status = "OK", accessToken = newAccessToken });
                    }
                    if (driver != null)
                    {
                        var request = db.ActiveRequests.Where(x => x.Request.Id == requestID).Include(x=>x.Request).Include(x=>x.AppropriateCar).FirstOrDefault();
                        if (request.AppropriateCar.Id == driver.Car.Id)
                        {
                            var distanceObject = GoogleAPIRequest.GetDistance(request.Request.StartingLocation, driver.Car.Location);

                            db.TakenRequests.Add(new TakenRequests()
                            {
                                Request = request.Request,
                                DateTimeTaken = DateTime.Now,
                                Car = driver.Car,
                                Company = driver.Company,
                                DriverLocation = new Models.Models.Location() { Latitude = driver.Car.Location.Latitude, Longitude = driver.Car.Location.Longitude },
                                DriverUserId = user,
                                DuractionText = distanceObject.rows[0].elements[0].duration.text,
                                DuractionValue = distanceObject.rows[0].elements[0].duration.value,
                                DistanceText = distanceObject.rows[0].elements[0].distance.text,
                                DistanceValue = distanceObject.rows[0].elements[0].distance.value
                            });

                            request.Request.RequestStatus = RequestStatusEnum.Taken;
                            request.AppropriateCar.CarStatus = CarStatus.Busy;
                            db.ActiveRequests.Remove(request);
                            db.SaveChanges();
                            
                            return Json(new { status = "OK", accessToken = newAccessToken });
                        }else
                        {
                            return Json(new { status = "ERR", accessToken = newAccessToken });
                        }
                    }
                    else
                    {
                        return Json(new { status = "ERR", accessToken = newAccessToken});
                    }
                }
                else
                {
                    return Json(new { status = "REMOVED", accessToken = newAccessToken });
                }
            }
            else
            {
                return Json(new { });
            }
        }

        public JsonResult UpdateStatus(double lat, double lon, string accessToken, int requestId, bool onAddress = false)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }
               

                if (db.TakenRequests.Any(x=> x.Request.Id == requestId))
                {
                     var user = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(y => y.Device).First().Device.UserId;
                    var request = db.TakenRequests.Where(x => x.Request.Id == requestId).FirstOrDefault();
                    if(request.DriverUserId == user)
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
                            return Json(new { status = "OK", clientInformed = true, accessToken = newAccessToken });
                        }
                        else
                        {
                            return Json(new { status = "OK", clientInformed = false, accessToken = newAccessToken });
                        }
                    }
                }
                return Json(new { status = "ERR", accessToken = newAccessToken });
            }
            else
            {
                return Json(new { });
            }
        }
        public JsonResult FinishRequest(int requestId, string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }
                var user = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(y => y.Device).First().Device.UserId;


                var request = db.TakenRequests.Where(x => x.Request.Id == requestId).Include(x=>x.Request).Include(x=>x.Car).FirstOrDefault();
                if (request != null) {
                    if (user == request.DriverUserId)
                    {
                        db.RequestHistory.Add(new RequestHistory()
                        {
                            Request = request.Request,
                            DriverUserId = request.DriverUserId,
                            Car = request.Car,
                            Company = request.Company,
                            DateTimeTaken = request.DateTimeTaken,
                            DateTimeFinished = DateTime.Now

                        });
                        request.Car.CarStatus = CarStatus.Free;
                        request.Request.RequestStatus = RequestStatusEnum.Finishted;
                        db.TakenRequests.Remove(request);
                        db.SaveChanges();

                        return Json(new { status = "OK", accessToken = newAccessToken });
                    }
                }
                return Json(new { status = "ERR", accessToken = newAccessToken });
            }
            else
            {
                return Json(new { });
            }

        }
        //public JsonResult GetAppropriateRequest(double lat, double lon, string accessToken)
        //{
        //    //TODO: basicAuth
        //    if (HttpContext.Request.RequestType == "POST")
        //    {
        //        var newAccessToken = AccessTokenStaticClass.GenerateAccessToken(accessToken);

        //        if (newAccessToken == null)
        //        {
        //            return Json(new { status = "INVALID ACCESSTOKEN" });
        //        }
        //        var user = db.AccessTokens.Where(x => x.UniqueAccesToken == newAccessToken).Include(y => y.Device).First().Device.UserId;
                
        //            bool haveCar = db.Drivers.Any(x => x.Car != null && x.UserId == user);
        //            bool haveCompany = db.Drivers.Any(x => x.Company != null && x.UserId == user);
        //            if (haveCar && haveCar)
        //            {
        //                var nearByRequests = db.ActiveRequests.Where(x => Math.Abs(x.Location.Latitude - lat) <= 0.0165 && Math.Abs(x.Location.Longitude - lon) <= 0.0165);

        //                List<object> requestList = new List<object>();
        //                foreach (var item in nearByRequests)
        //                {
        //                    var distance = Math.Round(DistanceBetweenTwoPoints.GetDistance(item.Location, new Models.Models.Location() { Latitude = lat, Longitude = lon }), 3);
        //                    if (distance < 1.5)
        //                    {
        //                        var address = GoogleAPIRequest.GetAddress(item.Location.Latitude, item.Location.Longitude);
        //                        requestList.Add(new { distance = distance, id = item.Id, address = address });
        //                    }
        //                }
        //                return Json(new { requests = requestList, status = "OK", accessToken = newAccessToken });
        //            }
              
        //        return Json(new { stutus = "NO PERMISSION", accessToken = newAccessToken });
        //    }
        //    else
        //    {
        //        return Json(new { });
        //    }
        //}

        public JsonResult GetInfoForRequest(int requestId, double lon, double lat, string basicAuth)
        {
            // TODO: basicAuth
            var request = db.ActiveRequests.Where(a => a.Id == requestId).Include(x=>x.Request).FirstOrDefault();
            var duration = GoogleAPIRequest.GetDistance(request.Request.StartingLocation, new Models.Models.Location() { Latitude = lat, Longitude = lon });
            return Json(new { lon = request.Request.StartingLocation.Longitude, lat = request.Request.StartingLocation.Latitude, distance = duration.rows[0].elements[0].distance.text, duration = duration.rows[0].elements[0].duration.text });
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
                if (db.ActiveRequests.Include(x => x.Request).Any(x => x.Request.CreatorUserId == user.Id))
                {
                    var record = db.ActiveRequests.Include(x => x.Request).Where(x => x.Request.CreatorUserId == user.Id).SingleOrDefault();
                    return Json(new { message = "The request was send for successfully", status = "OK", requestId = record.Id, address = address });
                }
                var request = new RequestInfo()
                {
                    StartingLocation = new Models.Models.Location() { Latitude = lat, Longitude = lon },
                    StartingAddress = address,
                    CreatedBy = CreatedBy.Client,
                    CreatedDateTime = DateTime.Now,
                    CreatorUserId = user.Id,
                    RequestStatus = RequestStatusEnum.NotTaken,
                    LastModificationDateTime = DateTime.Now
                };

                db.RequestsInfo.Add(request);

                //TODO: APPROPRIATE CAR

                var newItem = new ActiveRequests()
                {
                 Request = request
                };
                
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
