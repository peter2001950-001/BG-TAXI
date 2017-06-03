using System.Linq;
using System.Web.Http.Cors;
using System.Web.Mvc;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using BgTaxi.Web.ActionFilter;

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

        public RequestController(IRequestService requestService, ICarService carService, IDriverService driverService,
            IAccessTokenService accessTokenService)
        {
            this._requestService = requestService;
            this._carService = carService;
            this._driverService = driverService;
            this._accessTokenService = accessTokenService;
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
                    return Json(new {status = "INVALID ACCESSTOKEN"});

                var userId = _accessTokenService.GetUserId(newAccessToken);

                var driver = _driverService.GetDriverByUserId(userId);
                var car = _carService.GetCarByDriver(driver);
                _carService.UpdateCarInfo(car, new Location {Latitude = lat, Longitude = lon}, absent, free);

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

                return Json(new {status = "OK", carStatus = car.CarStatus, accessToken = newAccessToken});
            }
            return Json(new {});
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
                    return Json(new {status = "INVALID ACCESSTOKEN"});
                if (_requestService.GetActiveRequests().ToList().Any(x => x.Request.Id == requestID))
                {
                    var userId = _accessTokenService.GetUserId(newAccessToken);
                    var driver = _driverService.GetDriverByUserId(userId);
                    if (!answer)
                    {
                        _requestService.UpdateAnswer(false, requestID, driver);
                        return Json(new {status = "OK", accessToken = newAccessToken});
                    }

                    if (driver != null)
                        if (_requestService.UpdateAnswer(true, requestID, driver))
                            return Json(new {status = "OK", accessToken = newAccessToken});
                        else
                            return Json(new {status = "ERR", accessToken = newAccessToken});
                    return Json(new {status = "ERR", accessToken = newAccessToken});
                }
                return Json(new {status = "REMOVED", accessToken = newAccessToken});
            }
            return Json(new {});
        }


        public JsonResult GetAddress(string accessToken, string lat, string lng)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = _accessTokenService.GenerateAccessToken(accessToken);
                if(newAccessToken == null)
                    return Json(new { status = "INVALID ACCESSTOKEN" });

                var userId = _accessTokenService.GetUserId(newAccessToken);
                if (userId == null)
                    return Json(new { status = "ERR", accessToken = newAccessToken });
                double latDouble = 0; double lonDouble = 0;
                if(double.TryParse(lat, out latDouble) && double.TryParse(lng, out lonDouble))
                {
                    var address = PlacesAPI.GoogleRequests.GoogleAPIRequest.GetAddress(latDouble, lonDouble);
                    return Json(new { status = "OK", street_number = address.Street_number, street_address=address.Street_address, formattedAddress=address.FormattedAddress, accessToken = newAccessToken });
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
                    return Json(new {status = "INVALID ACCESSTOKEN"});
                var userId = _accessTokenService.GetUserId(newAccessToken);
                return Json(_requestService.FinishRequest(requestId, userId) ? new {status = "OK", accessToken = newAccessToken} : new {status = "ERR", accessToken = newAccessToken});
            }
            return Json(new {});
        }
    }
}