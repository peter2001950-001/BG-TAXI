using System.Linq;
using System.Web.Http.Cors;
using System.Web.Mvc;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using BgTaxi.Web.ActionFilter;
using BgTaxi.PlacesAPI.GoogleRequests;
using System;

namespace BgTaxi.Web.Controllers
{
    [AllowCrossSiteJson]
    [EnableCors("*", "*", "*")]
    public class RequestController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly ICarService _carService;
        private readonly IDriverService _driverService;
        private readonly IRequestService _requestService;
        private readonly IClientService _clientService;

        public RequestController(IRequestService requestService, ICarService carService, IDriverService driverService,
            IAccessTokenService accessTokenService, IClientService clientService)
        {
            this._requestService = requestService;
            this._carService = carService;
            this._driverService = driverService;
            this._accessTokenService = accessTokenService;
            this._clientService = clientService;
        }

        /// <summary>
        /// Updates the location of the car and takes care of the status as well as returns appropriate requests
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="accessToken"></param>
        /// <param name="absent"></param>
        /// <param name="free"></param>
        /// <param name="onAddress"></param>
        /// <returns></returns>
        public JsonResult Pull(double lon, double lat, string accessToken, bool absent = false, bool free = false,
            bool onAddress = false)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });

                var userId = _accessTokenService.GetUserId(newAccessToken);

                var driver = _driverService.GetDriverByUserId(userId);
                var car = _carService.GetCarByDriver(driver);
                _carService.UpdateCarInfo(car, new Models.Models.Location { Latitude = lat, Longitude = lon }, absent, free);

                if (onAddress)
                    if (_carService.CarOnAddress(car))
                        return
                            Json(
                                new
                                {
                                    status = "OK",
                                    onAddress = true,
                                    carStatus = car.CarStatus,
                                    accessToken = newAccessToken
                                });

                if (_carService.CarOnAddressCheck(car))
                    return
                        Json(
                            new
                            {
                                status = "OK",
                                onAddress = true,
                                carStatus = car.CarStatus,
                                accessToken = newAccessToken
                            });

                var requestObj = _requestService.AppropriateRequest(car);
                if (requestObj != null)
                    return
                        Json(
                            new
                            {
                                status = "OK",
                                accessToken = newAccessToken,
                                carStatus = car.CarStatus,
                                request = requestObj
                            });

                return Json(new { status = "OK", carStatus = car.CarStatus, accessToken = newAccessToken });
            }
            return Json(new { });
        }

        public JsonResult CatchUp(string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });

                var userId = _accessTokenService.GetUserId(newAccessToken);

                var driver = _driverService.GetDriverByUserId(userId);
                var car = _carService.GetCarByDriver(driver);
                return Json(new { request = _requestService.CatchUpRequest(car), status = "OK", accessToken = newAccessToken });
            }
            return Json(new { });
        }
        public JsonResult AutoSuggest(string accessToken, string query, string lat, string lng, string types, int radius = 15000)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);
                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });

                var userId = _accessTokenService.GetUserId(newAccessToken);
                if (userId == null)
                    return Json(new { status = "ERR", accessToken = newAccessToken });
                double latDouble = 0; double lonDouble = 0;
                lat.Replace('.', ',');
                lng.Replace('.', ',');
                if (double.TryParse(lat, out latDouble) && double.TryParse(lng, out lonDouble))
                {
                    var places = GoogleAPIRequest.AutoCompleteList(query, new Models.Models.Location() { Latitude = latDouble, Longitude = lonDouble }, radius, types);

                    return Json(new { status = "OK", places = places, accessToken = newAccessToken });
                }
                else
                {
                    return Json(new { status = "LatLon Parse Error", accessToken = newAccessToken });
                }
            }
            return Json(new { });
        }

        public JsonResult SendRequest(string accessToken, string startingAddressMainText, string startingAddressSecondaryText, string startingAddressLocationLat, string startingAddressLocationLng, string finishAddressMainText, string finishAddressSecondaryText, string finishAddressLocationLat, string finishAddressLocationLng)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);
                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });

                var userId = _accessTokenService.GetUserId(newAccessToken);
                if (userId == null)
                    return Json(new { status = "ERR1", accessToken = newAccessToken });
                var client = _clientService.GetAll().Where(x => x.UserId == userId).FirstOrDefault();
                if (client == null) return Json(new { status = "ERR2", accessToken = newAccessToken });
                var anotherRequest = _requestService.GetClientRequest().Where(c => c.Client.Id == client.Id).FirstOrDefault();
                if (anotherRequest != null) return Json(new { status = "ERR3", accessToken = accessToken });
                var requestInfo = new RequestInfo()
                {
                    CreatedBy = CreatedBy.Client,
                    CreatedDateTime = DateTime.Now,
                    CreatorUserId = userId,
                    LastModificationDateTime = DateTime.Now,
                    RequestStatus = RequestStatusEnum.NoCarChosen,
                    StartingAddress = startingAddressMainText + " " + startingAddressSecondaryText,
                    FinishAddress = finishAddressMainText + " " + finishAddressSecondaryText,
                    StartingLocation = new Models.Models.Location()
                    {
                        Latitude = double.Parse(startingAddressLocationLat),
                        Longitude = double.Parse(startingAddressLocationLng)
                    },
                    FinishLocation = new Models.Models.Location()
                    {
                        Latitude = double.Parse(finishAddressLocationLat),
                        Longitude = double.Parse(finishAddressLocationLng)
                    }

                };
                _requestService.AddRequestInfo(requestInfo);
                var activeRequest = new ActiveRequest();
                activeRequest.Request = requestInfo;
                activeRequest.DateTimeChosenCar = DateTime.Now;
               var appropriateCar = _carService.AppropriateCar(new Models.Models.Location() { Latitude = double.Parse(startingAddressLocationLat), Longitude = double.Parse(startingAddressLocationLng) });
                if (appropriateCar != null)
                {
                    activeRequest.AppropriateCar = appropriateCar;
                    requestInfo.RequestStatus = RequestStatusEnum.NotTaken;
                    _requestService.ModifyRequestInfo(requestInfo);
                }
                _requestService.AddActiveRequest(activeRequest);

                var clientRequest = new ClientRequest()
                {
                    Client = client,
                    Request = requestInfo
                };
                _requestService.AddClientRequest(clientRequest);
               
                return Json(new { accessToken = newAccessToken, status = "OK" });

            }
            return Json(new { });
        }

        public JsonResult ClientPull(string accessToken)
        {
            var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);
            if (newAccessToken == null)
                return Json(new { status = "INVALID ACCESSTOKEN" });

            var userId = _accessTokenService.GetUserId(newAccessToken);
            if (userId == null)
                return Json(new { status = "ERR1", accessToken = newAccessToken });
            var client = _clientService.GetAll().Where(x => x.UserId == userId).FirstOrDefault();
            if (client == null) return Json(new { status = "ERR2", accessToken = newAccessToken });
            _clientService.UpdateRequestStatus(userId);

            var clientRequest = _requestService.GetClientRequest().Where(x => x.Client.Id == client.Id).FirstOrDefault();
            if (clientRequest != null)
            {
                
                switch (clientRequest.Request.RequestStatus)
                {
                    case RequestStatusEnum.NotTaken:
                        return Json(new { status = "OK", code = 0, accessToken = newAccessToken });
                    case RequestStatusEnum.Taken:
                        var takenRequest = _requestService.GetTakenRequests().Where(x => x.Request.Id == clientRequest.Request.Id).FirstOrDefault();
                        if (takenRequest != null)
                        {
                            var car = _carService.GetCars().Where(x => x.Id == takenRequest.Car.Id).FirstOrDefault();
                            TimeSpan driff = DateTime.Now - takenRequest.DateTimeTaken;
                            return Json(new { status = "OK", code = 1, accessToken = newAccessToken, carNo = car.RegisterNumber, duration = takenRequest.DuractionValue-driff.TotalSeconds, carLng  = car.Location.Longitude, carLat = car.Location.Latitude });
                        }
                        return Json(new { status = "ERR5", accessToken = newAccessToken });
                    case RequestStatusEnum.Dismissed:

                        _requestService.RemoveClientRequest(clientRequest);
                        return Json(new { status = "OK", code =2, accessToken = newAccessToken });
                    case RequestStatusEnum.NoCarChosen:
                        return Json(new { status = "OK", code = 3, accessToken = newAccessToken });
                    case RequestStatusEnum.OnAddress:
                        return Json(new { status = "OK", code = 4, accessToken = newAccessToken });
                    case RequestStatusEnum.ClientConnected:
                        return Json(new { status = "OK", code = 6, accessToken = newAccessToken });

                    default: return Json(new { status = "ERR3", accessToken = newAccessToken });                
                    
                }
            }
            return Json(new { status = "OK", accessToken = newAccessToken, code = 5 });

        }

        public JsonResult CancelRequest(string accessToken)
        {
            var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);
            if (newAccessToken == null)
                return Json(new { status = "INVALID ACCESSTOKEN" });

            var userId = _accessTokenService.GetUserId(newAccessToken);
            if (userId == null)
                return Json(new { status = "ERR1", accessToken = newAccessToken });
            var client = _clientService.GetAll().Where(x => x.UserId == userId).FirstOrDefault();
            if (client == null) return Json(new { status = "ERR2", accessToken = newAccessToken });
            var clientRequest = _requestService.GetClientRequest().Where(x => x.Client.Id == client.Id).FirstOrDefault();
            if(clientRequest != null)
            {
                var activeRequest = _requestService.GetActiveRequests().Where(x => x.Request.Id == clientRequest.Request.Id).FirstOrDefault();
                if (activeRequest != null)
                {
                    _requestService.RemoveActiveRequest(activeRequest);
                    _requestService.RemoveClientRequest(clientRequest);
                    return Json(new {status = "OK", accessToken = newAccessToken });
                }
               
            }
            return Json(new { status = "ERR", accessToken = newAccessToken });
        }


        /// <summary>
        /// Update the request when the driver accept or deny the request
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="accessToken"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public JsonResult RequestAnswer(int requestID, string accessToken, bool answer)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                if (_requestService.GetActiveRequests().ToList().Any(x => x.Request.Id == requestID))
                {
                    var userId = _accessTokenService.GetUserId(newAccessToken);
                    var driver = _driverService.GetDriverByUserId(userId);
                    if (!answer)
                    {
                        _requestService.UpdateAnswer(false, requestID, driver);
                        return Json(new { status = "OK", accessToken = newAccessToken });
                    }

                    if (driver != null)
                        if (_requestService.UpdateAnswer(true, requestID, driver))
                            return Json(new { status = "OK", accessToken = newAccessToken });
                        else
                            return Json(new { status = "ERR", accessToken = newAccessToken });
                    return Json(new { status = "ERR", accessToken = newAccessToken });
                }
                return Json(new { status = "REMOVED", accessToken = newAccessToken });
            }
            return Json(new { });
        }


        public JsonResult GetAddress(string accessToken, string lat, string lng)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);
                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });

                var userId = _accessTokenService.GetUserId(newAccessToken);
                if (userId == null)
                    return Json(new { status = "ERR", accessToken = newAccessToken });
                double latDouble = 0; double lonDouble = 0;
                if (double.TryParse(lat, out latDouble) && double.TryParse(lng, out lonDouble))
                {
                    var address = PlacesAPI.GoogleRequests.GoogleAPIRequest.GetAddress(latDouble, lonDouble);
                    return Json(new { status = "OK", street_number = address.Street_number, street_address = address.Street_address, formattedAddress = address.FormattedAddress, accessToken = newAccessToken });
                }
                return Json(new { status = "ERR", accessToken = newAccessToken });
            }
            return Json(new { });
        }
        public JsonResult FinishRequest(int requestId, string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                var userId = _accessTokenService.GetUserId(newAccessToken);
                return Json(_requestService.FinishRequest(requestId, userId) ? new { status = "OK", accessToken = newAccessToken } : new { status = "ERR", accessToken = newAccessToken });
            }
            return Json(new { });
        }
    }
}