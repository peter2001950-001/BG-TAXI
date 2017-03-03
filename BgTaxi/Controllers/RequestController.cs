using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using BgTaxi.Web.ActionFilter;
using System.Linq;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace BgTaxi.Web.Controllers
{
    [AllowCrossSiteJsonAttribute]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RequestController : Controller
    {

        private readonly IRequestService requestService;
        private readonly ICarService carService;
        private readonly IDriverService driverService;
        private readonly IAccessTokenService accessTokenService;

        public RequestController(IRequestService requestService, ICarService carService, IDriverService driverService, IAccessTokenService accessTokenService)
        {
            this.requestService = requestService;
            this.carService = carService;
            this.driverService = driverService;
            this.accessTokenService = accessTokenService;
        }
        
        public JsonResult Pull(double lon, double lat, string accessToken, bool absent = false, bool free = false, bool onAddress = false)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = accessTokenService.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }

                var userId = accessTokenService.GetUserId(newAccessToken);
                
                var driver = driverService.GetDriverByUserId(userId);
                Car car = carService.GetCarByDriver(driver);
                carService.UpdateCarInfo(car, new Models.Models.Location() { Latitude = lat, Longitude = lon }, absent, free);

                if (onAddress)
                {
                    if (carService.CarOnAddress(car))
                    {
                        return Json(new { status = "OK", onAddress = true, accessToken = newAccessToken });
                    }
                   
                }

                if (carService.CarOnAddressCheck(car))
                {
                    return Json(new { status = "OK", onAddress = true, accessToken = newAccessToken });
                }

                var requestObj = requestService.AppropriateRequest(car);
                if(requestObj!=null)
                {
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
                var newAccessToken = accessTokenService.GenerateAccessToken(accessToken);

                    if (newAccessToken == null)
                    {
                        return Json(new { status = "INVALID ACCESSTOKEN" });
                    }
                if (requestService.GetActiveRequests().Any(x => x.Request.Id == requestID))
                {
                    
                    var userId = accessTokenService.GetUserId(newAccessToken);
                    var driver = driverService.GetDriverByUserId(userId);
                    if (!answer)
                    {
                        requestService.UpdateAnswer(answer, requestID, driver);
                        return Json(new { status = "OK", accessToken = newAccessToken });
                    }

                    if (driver != null)
                    {
                       if(requestService.UpdateAnswer(answer, requestID, driver))
                        {
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

   
        public JsonResult FinishRequest(int requestId, string accessToken)
        {
            if (HttpContext.Request.RequestType == "POST")
            {
                var newAccessToken = accessTokenService.GenerateAccessToken(accessToken);

                if (newAccessToken == null)
                {
                    return Json(new { status = "INVALID ACCESSTOKEN" });
                }
                var userId = accessTokenService.GetUserId(newAccessToken);
                if(requestService.FinishRequest(requestId, userId)) { 

                        return Json(new { status = "OK", accessToken = newAccessToken });
                }else {
                    return Json(new { status = "ERR", accessToken = newAccessToken });
                }
            }
            else
            {
                return Json(new { });
            }

        }
     
        

    }
}
