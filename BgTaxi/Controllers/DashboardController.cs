using BgTaxi.Models.Models;
using BgTaxi.PlacesAPI.GoogleRequests;
using BgTaxi.Services.Contracts;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using BgTaxi.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace BgTaxi.Controllers
{

    [Authorize(Roles = "Dispatcher")]
    public class DashboardController : Controller
    {
        private readonly IDispatcherService _dispatcherService;
        private readonly IDashboardService _dashboardService;
        private readonly ICarService _carService;
        private readonly ICompanyService _companyService;
        private readonly IRequestService _requestService;
        /// <summary>
        /// Application DB context
        /// </summary>
        protected Models.ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<Models.ApplicationUser> UserManager { get; set; }

        public DashboardController (IDispatcherService dispatcherService, IDashboardService dashboardService, ICarService carService, ICompanyService companyService, IRequestService requestService)
        {
            this._dispatcherService = dispatcherService;
            this._dashboardService = dashboardService;
            this._carService = carService;
            this._companyService = companyService;
            this._requestService = requestService;
            this.ApplicationDbContext = new Models.ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }

        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Controls all requests of the dispacher, takes care of the communication between the drivers and dispatcher
        /// </summary>
        /// <param name="free"></param>
        /// <param name="busy"></param>
        /// <param name="absent"></param>
        /// <param name="offline"></param>
        /// <param name="offduty"></param>
        /// <returns>Retuens car locations and statuses, all active request and their statuses</returns>
        public JsonResult Pull(bool free = false, bool busy = false, bool absent = false, bool offline = false, bool offduty= false)
        {
            var userId = User.Identity.GetUserId();
            _dashboardService.UpdateRequestStatus(userId);
            var requests = _dashboardService.GetRequests(userId);

            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var dispatcherCompanyId = dispatcher.Company.Id;
            var cars = _carService.GetCars().Where(x => x.Company.Id == dispatcher.Company.Id).ToList();

            List<object> carsList = new List<object>();
            object[] carObjs = new object[cars.Count];
            int freeStatusCount = 0;
            int busyStatusCount = 0;
            int absentStatusCount = 0;
            int offlineStatusCount = 0;
            int offdutyStatusCount = 0;
            var addCar = false;
            for (int i = 0; i < cars.Count; i++)
            {
                addCar = false;
                TimeSpan diff = DateTime.Now - cars[i].LastActiveDateTime;
                if (diff.TotalMinutes > 2 && cars[i].CarStatus != CarStatus.OffDuty)
                {
                    cars[i].CarStatus = CarStatus.Offline;
                    _carService.SaveChanges();
                }
                switch (cars[i].CarStatus)
                {
                    case CarStatus.Free:
                        if (free)
                        {
                            addCar = true;
                        }
                        freeStatusCount++;
                        break;
                    case CarStatus.Busy:
                        if (busy)
                        {
                            addCar = true;
                        }
                        busyStatusCount++;
                        break;
                    case CarStatus.Absent:
                        if (absent)
                        {
                            addCar = true;
                        }
                        absentStatusCount++;
                        break;
                    case CarStatus.OffDuty:
                        if (offduty)
                        {
                            addCar = true;
                        }
                        offdutyStatusCount++;
                        break;
                    case CarStatus.Offline:
                        if (offline)
                        {
                            addCar = true;
                        }
                        offlineStatusCount++;
                        break;
                    default:
                        break;
                }

                if (addCar)
                {
                    carsList.Add(new { lng = cars[i].Location.Longitude, lat = cars[i].Location.Latitude, id = cars[i].InternalNumber, carStatus = cars[i].CarStatus });
                }
            }
            carObjs = carsList.ToArray();
            return Json(new { requests = requests, cars = carObjs, freeStatusCount = freeStatusCount, busyStatusCount = busyStatusCount, absentStatusCount = absentStatusCount, offlineStatusCount = offlineStatusCount, offdutyStatusCount = offdutyStatusCount });
        }
    

        /// <summary>
        /// Returns requestLocation with the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
              public JsonResult RequestLocation(string id)
        {
            var userId = User.Identity.GetUserId();
            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var dispatcherCompanyId = dispatcher.Company.Id;
            var requestId = 0;
            int.TryParse(id, out requestId);
            var foundRequest = _requestService.GetRequestInfos().Where(x => x.Id == requestId && x.Company.Id == dispatcherCompanyId).FirstOrDefault();

            if (foundRequest != null)
            {
                return Json(new { status = "OK", lat = foundRequest.StartingLocation.Latitude, lng = foundRequest.StartingLocation.Longitude });
            }
            return Json(new { status = "ERR" });
        }

        /// <summary>
        /// Returns additional information about the request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult RequestInfo(string id)
        {
            var userId = User.Identity.GetUserId();
            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var dispatcherCompanyId = dispatcher.Company.Id;

            var requestId = 0;
            int.TryParse(id, out requestId);

            var foundRequest = _requestService.GetRequestInfos().Where(x => x.Id == requestId && x.Company.Id == dispatcherCompanyId).FirstOrDefault();
            var creator = UserManager.FindById(foundRequest.CreatorUserId);

            var requestInfo = new Dictionary<string, object>();

            if (foundRequest != null)
            {
                requestInfo.Add("status", "OK");
                requestInfo.Add("id", id);
                requestInfo.Add("startingAddress", foundRequest.StartingAddress);
                requestInfo.Add("finishAddress", foundRequest.FinishAddress);
                requestInfo.Add("requestStatus", foundRequest.RequestStatus);
                requestInfo.Add("createdBy", creator.FirstName + " " + creator.LastName);
                requestInfo.Add("createdDateTime", foundRequest.CreatedDateTime.AddHours(10).ToString("dd/MM/yyyy \"г. \" HH:mm:ss"));
                if (foundRequest.RequestStatus == RequestStatusEnum.Finishted)
                {
                    var requestHistory = _requestService.GetRequestHistories().Where(x => x.Request.Id == foundRequest.Id).FirstOrDefault();
                    if (requestHistory != null)
                    {
                        string carInternalNum = null;
                        var car = _carService.GetCars().Where(x => x.Id == requestHistory.Car.Id).FirstOrDefault();
                        if (car != null)
                        {
                            carInternalNum = car.InternalNumber;
                        }
                        var takenDriver = UserManager.FindById(requestHistory.DriverUserId);

                        requestInfo.Add("takenBy", takenDriver.FirstName + " " + takenDriver.LastName);
                        requestInfo.Add("takenByCar", carInternalNum);
                    }
                }else if(foundRequest.RequestStatus == RequestStatusEnum.ClientConnected && foundRequest.RequestStatus== RequestStatusEnum.OnAddress && foundRequest.RequestStatus == RequestStatusEnum.ClientConnected)
                {
                    var takenRequest = _requestService.GetTakenRequests().Where(x => x.Request.Id == foundRequest.Id).FirstOrDefault();
                    if (takenRequest != null)
                    {
                        string carInternalNum = null;
                        var car = _carService.GetCars().Where(x => x.Id == takenRequest.Car.Id).FirstOrDefault();
                        if (car != null)
                        {
                            carInternalNum = car.InternalNumber;
                        }
                        var takenDriver = UserManager.FindById(takenRequest.DriverUserId);

                        requestInfo.Add("takenBy", takenDriver.FirstName + " " + takenDriver.LastName);
                        requestInfo.Add("takenByCar", carInternalNum);
                    }
                }else if(foundRequest.RequestStatus == RequestStatusEnum.Taken)
                {
                    var takenRequest = _requestService.GetTakenRequests().Where(x => x.Request.Id == foundRequest.Id).FirstOrDefault();
                    if (takenRequest != null)
                    {
                        string carInternalNum = null;
                        var car = _carService.GetCars().Where(x => x.Id == takenRequest.Car.Id).FirstOrDefault();
                        if (car != null)
                        {
                            carInternalNum = car.InternalNumber;
                        }
                       var distance =  GoogleAPIRequest.GetDistance(car.Location, foundRequest.StartingLocation);
                        var takenDriver = UserManager.FindById(takenRequest.DriverUserId);

                        requestInfo.Add("takenBy", takenDriver.FirstName + " " + takenDriver.LastName);
                        requestInfo.Add("takenByCar", carInternalNum);
                        requestInfo.Add("duration", distance.rows[0].elements[0].duration.text);
                        requestInfo.Add("distance", distance.rows[0].elements[0].distance.text);
                    }
                }
            }else
            {
                requestInfo.Add("status", "ERR");
            }

            string json = JsonConvert.SerializeObject(requestInfo, Formatting.Indented);

            return Json(json);
        }

        /// <summary>
        /// Search requests by id, period or startingAddress
        /// </summary>
        /// <param name="id"></param>
        /// <param name="period"></param>
        /// <param name="startingAddress"></param>
        /// <returns></returns>
        public JsonResult SearchRequest(string id="0", string period="0", string startingAddress=null)
        {

            var userId = User.Identity.GetUserId();
            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var dispatcherCompanyId = dispatcher.Company.Id;
            if(startingAddress == "undefined")
            {
                startingAddress = null;
            }
            if(period== "undefined")
            {
                period = null;
            }
            int requestId = 0;
            int.TryParse(id, out requestId);
            int requestPeriod = 0;
            int.TryParse(period, out requestPeriod);
            var minDateTime = DateTime.Now;
            List<object> requestsFound = new List<object>();

            if (requestPeriod != 0)
            {
               minDateTime = DateTime.Now.AddHours(-requestPeriod);
            }
            if (requestId > 0)
            {
               var foundRequest =  _requestService.GetRequestInfos().Where(x => x.Id == requestId && x.Company.Id == dispatcherCompanyId).FirstOrDefault();
                if (foundRequest != null)
                {
                    var foundAddress = foundRequest.StartingAddress.ToLower();
                    var matched = true;
                    if (period != null)
                    {
                        if(foundRequest.CreatedDateTime < minDateTime) matched = false;
                    }
                    if (startingAddress != null)
                    {
                        if (foundAddress.IndexOf(startingAddress.ToLower()) == -1) matched = false;
                    }
                    if (matched) { 
                        requestsFound.Add(new { id = foundRequest.Id, startingAddress = foundRequest.StartingAddress, finishAddress = foundRequest.FinishAddress, requestStatus = foundRequest.RequestStatus });
                    }
                }
            }else if (startingAddress != null)
            {
                var foundRequests = _requestService.GetRequestInfos().Where(x => x.StartingAddress.ToLower().Contains(startingAddress.ToLower()) && x.CreatedDateTime>=minDateTime &&x.Company.Id == dispatcherCompanyId).ToList();
                foreach (var item in foundRequests)
                    {
                    requestsFound.Add(new { id = item.Id, startingAddress = item.StartingAddress, finishAddress = item.FinishAddress, requestStatus = item.RequestStatus });
                }
            }else if (requestPeriod != 0)
            {
                var foundRequests = _requestService.GetRequestInfos().Where(x=> x.CreatedDateTime >= minDateTime && x.Company.Id == dispatcherCompanyId).ToList();
                foreach (var item in foundRequests)
                {
                    requestsFound.Add(new { id = item.Id, startingAddress = item.StartingAddress, finishAddress = item.FinishAddress, requestStatus = item.RequestStatus });
                }
            }
            return Json(new { status = "OK", requests = requestsFound.ToArray() });
        }
        /// <summary>
        /// Returns suggested places or addresses
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public JsonResult AutoComplete(string text)
        {
            var userId = User.Identity.GetUserId();
            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var company = _companyService.GetAll().First(x => x.Id == dispatcher.Company.Id);

            var cityLocation = company.CityLocation;
            var suggestions = GoogleAPIRequest.AutoCompleteList(text, cityLocation, 10000, "");

            object[] result = new object[suggestions.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] =
                    new
                    {
                        main_text = suggestions[i].MainText,
                        secondary_text = suggestions[i].Address,
                        placeId = suggestions[i].PlaceId
                    };

            }
            return Json(new {status = "OK", suggestions = result});

        }
        /// <summary>
        /// Adds a new request
        /// </summary>
        /// <param name="startingAddress"></param>
        /// <param name="finishAddress"></param>
        /// <returns></returns>
        public JsonResult CreateRequest(string startingAddress, string finishAddress)
        {
            var userId = User.Identity.GetUserId();
            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var company = _companyService.GetAll().First(x => x.Id == dispatcher.Company.Id);

            var startingLocation = GoogleAPIRequest.GetLocation(startingAddress + " " + company.City);
            var finishLocation = GoogleAPIRequest.GetLocation(finishAddress + " " + company.City);
            if (startingLocation == null)
            {
                return Json(new { status = "INVALID STARTING LOCATION" });
            }
            if(finishLocation == null)
            {
                finishLocation = new Models.Models.Location()
                {
                    Latitude = 0,
                    Longitude = 0
                };
            }
            
            
           
            var request = new RequestInfo()
            {
                CreatedBy = CreatedBy.Dispatcher,
                CreatorUserId = userId,
                RequestStatus = RequestStatusEnum.NoCarChosen,
                StartingAddress = startingAddress,
                StartingLocation = startingLocation,
                FinishAddress = finishAddress,
                FinishLocation = finishLocation,
                CreatedDateTime = DateTime.Now,
                LastModificationDateTime = DateTime.Now, 
                Company = company
            };
            _requestService.AddRequestInfo(request);
            var dashboardCell = new DispatcherDashboard()
            {
                DispatcherUserId = userId,
                LastSeen = DateTime.Now,
                LastSeenStatus = RequestStatusEnum.NoCarChosen,
                Request = request

            };

            _dashboardService.AddDispatcherDashboard(dashboardCell);
            var activeRequest = new ActiveRequest()
            {
                AppropriateCar = null,
                DateTimeChosenCar = DateTime.Now,
                Request = request
            };
            _requestService.AddActiveRequest(activeRequest);
            var car = _carService.AppropriateCar(startingLocation, dispatcher.Company);
            if (car != null)
            {
                request.RequestStatus = RequestStatusEnum.NotTaken;
                activeRequest.AppropriateCar = car;
                 _requestService.ModifyActiveRequest(activeRequest);
            _requestService.ModifyRequestInfo(request);
            }
           
           


            return Json(new { status = "OK" });
        }

     
    }
}