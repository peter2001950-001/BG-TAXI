using BgTaxi.Models.Models;
using BgTaxi.Web.GoogleRequests;
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
        Models.Models.Database db = new Models.Models.Database();

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult Pull()
        {
            var userId = User.Identity.GetUserId();
            var dashboard = db.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x=>x.Request);
            var dashboardlist = dashboard.ToList();

            foreach (var item in dashboardlist)
            {
                if (item.LastSeenStatus == RequestStatusEnum.Taken)
                {
                    TimeSpan diff = DateTime.Now - item.LastSeen;
                    if (diff.TotalSeconds > 60)
                    {
                        var dispatcherDashboardRequest = dashboard.Where(x => x.Id == item.Id).First();
                        db.DispatchersDashboard.Remove(dispatcherDashboardRequest);
                    }
                }
                else if(item.LastSeenStatus == RequestStatusEnum.Dismissed)
                {
                    TimeSpan diff = DateTime.Now - item.LastSeen;
                    if (diff.TotalSeconds > 60)
                    {
                        var activeeRequest = db.ActiveRequests.Where(x => x.Request.Id == item.Request.Id).First();
                        var dispatcherDashboardRequest = dashboard.Where(x => x.Id == item.Id).First();
                        db.ActiveRequests.Remove(activeeRequest);
                        db.DispatchersDashboard.Remove(dispatcherDashboardRequest);
                      
                    }
                }
            }
            db.SaveChanges();
            var requestsNotList = db.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request);
            var requestsList = requestsNotList.ToList();
            object[] requests = new object[requestsList.Count];
            for (int i = 0; i < requestsList.Count; i++)
            {
                
                string carId = null;
                string duraction = null;
                var reqId = requestsList[i].Request.Id;
                switch (requestsList[i].Request.RequestStatus)
                {
                    case RequestStatusEnum.NotTaken:
                        var activeRequest = db.ActiveRequests.Where(x => x.Request.Id == reqId).Include(x => x.Request).Include(x=>x.AppropriateCar).FirstOrDefault();
                        TimeSpan diff = DateTime.Now - activeRequest.DateTimeChosenCar;
                        if (diff.TotalSeconds > 15)
                        {
                            db.CarsDismissedRequests.Add(new CarDismissedRequest()
                            {
                                Car = activeRequest.AppropriateCar,
                                Request = activeRequest.Request
                            });
                            db.SaveChanges();
                                var newCar = AppropriateCar(activeRequest.Request.StartingLocation, activeRequest.Request);
                            if (newCar == null)
                            {
                                activeRequest.Request.RequestStatus = RequestStatusEnum.NoCarChosen;
                                activeRequest.AppropriateCar = null;
                                activeRequest.DateTimeChosenCar = DateTime.Now;
                            }
                            else
                            {
                                activeRequest.AppropriateCar = newCar;
                                activeRequest.DateTimeChosenCar = DateTime.Now;
                            }
                        }
                        break;
                    case RequestStatusEnum.Taken:
                        var requesttID = requestsList[i].Request.Id;
                        var takenRequest = db.TakenRequests.Where(x => x.Request.Id == requesttID).Include(x => x.Car).FirstOrDefault();
                        
                        duraction = takenRequest.DuractionText;
                        carId = takenRequest.Car.InternalNumber;
                        break;
                    case RequestStatusEnum.NoCarChosen:
                       
                        var actReque = db.ActiveRequests.Where(x => x.Request.Id == reqId).Include(x => x.Request).FirstOrDefault();
                        TimeSpan diff1 = DateTime.Now - actReque.DateTimeChosenCar;
                        if (diff1.TotalSeconds > 90)
                        {
                            actReque.Request.RequestStatus = RequestStatusEnum.Dismissed;
                        }
                        else
                        {
                            var newCar = AppropriateCar(actReque.Request.StartingLocation, actReque.Request);
                            if (newCar != null)
                            {
                                actReque.AppropriateCar = newCar;
                                actReque.DateTimeChosenCar = DateTime.Now;
                                actReque.Request.RequestStatus = RequestStatusEnum.NotTaken;
                            }
                        }
                            break;
                    case RequestStatusEnum.OnAddress:
                        break;
                    case RequestStatusEnum.ClientConnected:
                        break;
                    case RequestStatusEnum.MissingClient:
                        break;
                    case RequestStatusEnum.Finishted:
                        break;
                    default:
                        break;
                }
                
                requests[i] = new {id = requestsList[i].Request.Id, startingAddress = requestsList[i].Request.StartingAddress, finishAddress = requestsList[i].Request.FinishAddress, requestStatus = requestsList[i].Request.RequestStatus, carId = carId, duraction = duraction };
               
            }
            db.SaveChanges();
            for (int i = 0; i < requestsList.Count; i++)
            {
                var requestId = requestsList[i].Id;
                var currentRequest = db.DispatchersDashboard.Where(x => x.Id == requestId).Include(x=>x.Request).FirstOrDefault();
                if (currentRequest.Request.RequestStatus != currentRequest.LastSeenStatus)
                {
                    currentRequest.LastSeen = DateTime.Now;
                    currentRequest.LastSeenStatus = requestsList[i].Request.RequestStatus;
                }

            }

            db.SaveChanges();
           
           

            return Json(new { requests = requests });
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
            var car = AppropriateCar(startingLocation);
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
                LastModificationDateTime = DateTime.Now
            };
            db.RequestsInfo.Add(request);
            db.SaveChanges();
            var dashboardCell = new DispatcherDashboard()
            {
                DispatcherUserId = userId,
                LastSeen = DateTime.Now,
                LastSeenStatus = RequestStatusEnum.NotTaken,
                Request = request

            };

            db.DispatchersDashboard.Add(dashboardCell);
            if (car == null)
            {
                request.RequestStatus = RequestStatusEnum.NoCarChosen;
            }

            var activeRequest = new ActiveRequests()
            {
                AppropriateCar = car,
                DateTimeChosenCar = DateTime.Now,
                Request = request
            };
            db.ActiveRequests.Add(activeRequest);

            db.SaveChanges();


            return Json(new { status = "OK" });
        }

        private Car AppropriateCar(Models.Models.Location startingLocaion)
        {
           var nearBycars =  db.Cars.Where(x => x.CarStatus == CarStatus.Free).Where(x => Math.Abs(x.Location.Latitude - startingLocaion.Latitude) <= 0.0300 && Math.Abs(x.Location.Longitude - startingLocaion.Longitude) <= 0.0300).ToList();
            List<double> distances = new List<double>();
            Dictionary<double, Car> dictionary = new Dictionary<double, Car>();
            Car appropriateCar = null;
            if(nearBycars.Count == 0)
            {
                return null;
            }
            foreach (var item in nearBycars)
            {
                var distance = DistanceBetweenTwoPoints.GetDistance(startingLocaion, item.Location);
                distances.Add(distance);
                dictionary.Add(distance, item);
            }

            distances.OrderBy(x => x);
            foreach (var item in distances)
            {
                 var car = dictionary[item];
                if(!db.ActiveRequests.Any(x=>x.AppropriateCar.Id == car.Id))
                {
                    appropriateCar = car;
                    break;
                }
            }
           

            return appropriateCar;
        }
        private Car AppropriateCar(Models.Models.Location startingLocaion, RequestInfo request)
        {
            var nearBycars = db.Cars.Where(x => x.CarStatus == CarStatus.Free).Where(x => Math.Abs(x.Location.Latitude - startingLocaion.Latitude) <= 0.0300 && Math.Abs(x.Location.Longitude - startingLocaion.Longitude) <= 0.0300).ToList();
            List<double> distances = new List<double>();
            Dictionary<double, Car> dictionary = new Dictionary<double, Car>();
            Car chosenCar = null;
            if (nearBycars.Count == 0)
            {
                return null;
            }
            foreach (var item in nearBycars)
            {
                var distance = DistanceBetweenTwoPoints.GetDistance(startingLocaion, item.Location);
                distances.Add(distance);
                dictionary.Add(distance, item);
            }

            distances.OrderBy(x => x);
            foreach (var item in distances)
            {
                var car = dictionary[item];
                if(!(db.CarsDismissedRequests.Where(x=>x.Request.Id == request.Id).Any(x=>x.Car.Id == car.Id)) && !(db.ActiveRequests.Any(x => x.AppropriateCar.Id == car.Id)))
                {
                    chosenCar = car;
                }
            }
           

            return chosenCar;
        }
    }
}