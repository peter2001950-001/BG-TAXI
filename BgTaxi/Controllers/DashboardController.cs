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

namespace BgTaxi.Controllers
{

    [Authorize(Roles = "Dispatcher")]
    public class DashboardController : Controller
    {
        private readonly IDispatcherService dispatcherService;
        private readonly IDashboardService dashboardService;
        private readonly ICarService carService;
        private readonly ICompanyService companyService;
        private readonly IRequestService requestService;
        public DashboardController (IDispatcherService dispatcherService, IDashboardService dashboardService, ICarService carService, ICompanyService companyService, IRequestService requestService)
        {
            this.dispatcherService = dispatcherService;
            this.dashboardService = dashboardService;
            this.carService = carService;
            this.companyService = companyService;
            this.requestService = requestService;
        }

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult Pull()
        {
            var userId = User.Identity.GetUserId();
            dashboardService.UpdateRequestStatus(userId);
            var requests = dashboardService.GetRequests(userId);

            var dispatcher = dispatcherService.GetAll().Where(x => x.UserId == userId).First();
            var dispatcherCompanyId = dispatcher.Company.Id;
            var cars = carService.GetCars().Where(x => x.Company.Id == dispatcher.Company.Id).ToList();

            object[] carObjs = new object[cars.Count];
            int freeStatusCount = 0;
            int busyStatusCount = 0;
            int absentStatusCount = 0;
            int offlineStatusCount = 0;
            int offdutyStatusCount = 0;
            for (int i = 0; i < cars.Count; i++)
            {
                TimeSpan diff = DateTime.Now - cars[i].LastActiveDateTime;
                if (diff.TotalMinutes >2  && cars[i].CarStatus != CarStatus.OffDuty)
                {
                    cars[i].CarStatus = CarStatus.Offline;
                    carService.SaveChanges();
                }
                switch (cars[i].CarStatus)  
                {
                    case CarStatus.Free:
                        freeStatusCount++;
                        break;
                    case CarStatus.Busy:
                        busyStatusCount++;
                        break;
                    case CarStatus.Absent:
                        absentStatusCount++;
                        break;
                    case CarStatus.OffDuty:
                        offdutyStatusCount++;
                        break;
                    case CarStatus.Offline:
                        offlineStatusCount++;
                        break;
                    default:
                        break;
                }
                
                carObjs[i] = new { lng = cars[i].Location.Longitude, lat = cars[i].Location.Latitude, id = cars[i].InternalNumber, carStatus = cars[i].CarStatus };
            }
            return Json(new { requests = requests, cars = carObjs, freeStatusCount = freeStatusCount, busyStatusCount = busyStatusCount, absentStatusCount = absentStatusCount, offlineStatusCount = offlineStatusCount, offdutyStatusCount = offdutyStatusCount });
        }

        public JsonResult CreateRequest(string startingAddress, string finishAddress)
        {
            var startingLocation = GoogleAPIRequest.GetLocation(startingAddress);
            var finishLocation = GoogleAPIRequest.GetLocation(finishAddress);
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
            

            var userId = User.Identity.GetUserId();
            var dispatcher = dispatcherService.GetAll().Where(x => x.UserId == userId).First();
            var company = companyService.GetAll().Where(x => x.Id == dispatcher.Company.Id).First();
            var car = dashboardService.AppropriateCar(startingLocation,  dispatcher.Company);
            var request = new RequestInfo()
            {
                CreatedBy = CreatedBy.Dispatcher,
                CreatorUserId = userId,
                RequestStatus = RequestStatusEnum.NotTaken,
                StartingAddress = startingAddress,
                StartingLocation = startingLocation,
                FinishAddress = finishAddress,
                FinishLocation = finishLocation,
                CreatedDateTime = DateTime.Now,
                LastModificationDateTime = DateTime.Now, 
                Company = company
            };
            requestService.AddRequestInfo(request);
            var dashboardCell = new DispatcherDashboard()
            {
                DispatcherUserId = userId,
                LastSeen = DateTime.Now,
                LastSeenStatus = RequestStatusEnum.NotTaken,
                Request = request

            };

            dashboardService.AddDispatcherDashboard(dashboardCell);
            if (car == null)
            {
                request.RequestStatus = RequestStatusEnum.NoCarChosen;
            }

            var activeRequest = new ActiveRequest()
            {
                AppropriateCar = car,
                DateTimeChosenCar = DateTime.Now,
                Request = request
            };
            requestService.AddActiveRequest(activeRequest);


            return Json(new { status = "OK" });
        }

     
    }
}