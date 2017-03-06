using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BgTaxi.Models.Models;
using BgTaxi.Models;
using System.Data.Entity;
using BgTaxi.PlacesAPI.GoogleRequests;

namespace BgTaxi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDatabase data;
        public DashboardService(IDatabase data)
        {
            this.data = data;
        }
        public IEnumerable<DispatcherDashboard> GetAll()
        {
            return data.DispatchersDashboard.AsEnumerable();
        }

        public object[] GetRequests(string userId)
        {

            var requestsList = data.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request).ToList();
            object[] requests = new object[requestsList.Count];

            for (int i = 0; i < requestsList.Count; i++)
            {
                string duraction = null;
                string carId = null;
                if (requestsList[i].Request.RequestStatus == RequestStatusEnum.Taken)
                {
                    var requesttID = requestsList[i].Request.Id;
                    var takenRequest = data.TakenRequests.Where(x => x.Request.Id == requesttID).Include(x => x.Car).FirstOrDefault();

                    duraction = takenRequest.DuractionText;
                    carId = takenRequest.Car.InternalNumber;
                }
                requests[i] = new { id = requestsList[i].Request.Id, startingAddress = requestsList[i].Request.StartingAddress, finishAddress = requestsList[i].Request.FinishAddress, requestStatus = requestsList[i].Request.RequestStatus, carId = carId, duraction = duraction };
            }
            return requests;
        }

        public void UpdateRequestStatus(string userId)
        {
            var requestsList = data.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request).ToList();
            var dashboard = data.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request).ToList();
            foreach (var item in dashboard)
            {
                if (item.LastSeenStatus != RequestStatusEnum.Dismissed)
                {
                    TimeSpan diff = DateTime.Now - item.LastSeen;
                    if (diff.TotalSeconds > 60)
                    {
                        var dispatcherDashboardRequest = dashboard.Where(x => x.Id == item.Id).First();
                        data.DispatchersDashboard.Remove(dispatcherDashboardRequest);
                    }
                }
                else if (item.LastSeenStatus == RequestStatusEnum.Dismissed)
                {
                    TimeSpan diff = DateTime.Now - item.LastSeen;
                    if (diff.TotalSeconds > 60)
                    {
                        var activeeRequest = data.ActiveRequests.Where(x => x.Request.Id == item.Request.Id).First();
                        var dispatcherDashboardRequest = dashboard.Where(x => x.Id == item.Id).First();
                        data.ActiveRequests.Remove(activeeRequest);
                        data.DispatchersDashboard.Remove(dispatcherDashboardRequest);

                    }
                }
            }

            for (int i = 0; i < dashboard.Count; i++)
            {
                var reqId = dashboard[i].Request.Id;
                if (dashboard[i].Request.RequestStatus == RequestStatusEnum.NotTaken)
                {
                    NotTakenRequest(reqId);

                } else if(dashboard[i].Request.RequestStatus == RequestStatusEnum.NoCarChosen)
                {
                    NoCarChosen(reqId);
                }
                   
            }
                
            
            data.SaveChanges();
            for (int i = 0; i < requestsList.Count; i++)
            {
                var requestId = requestsList[i].Id;
                var currentRequest = data.DispatchersDashboard.Where(x => x.Id == requestId).Include(x => x.Request).FirstOrDefault();
                if (currentRequest.Request.RequestStatus != currentRequest.LastSeenStatus)
                {
                    currentRequest.LastSeen = DateTime.Now;
                    currentRequest.LastSeenStatus = requestsList[i].Request.RequestStatus;
                }

            }
        }

        public Car AppropriateCar(Models.Models.Location startingLocaion, Company company)
        {
            var nearBycars = data.Cars.Where(x => x.Company.Id == company.Id).Where(x => x.CarStatus == CarStatus.Free).Where(x => Math.Abs(x.Location.Latitude - startingLocaion.Latitude) <= 0.0300 && Math.Abs(x.Location.Longitude - startingLocaion.Longitude) <= 0.0300).ToList();
            List<double> distances = new List<double>();
            Dictionary<double, Car> dictionary = new Dictionary<double, Car>();
            Car appropriateCar = null;
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
                if (!data.ActiveRequests.Any(x => x.AppropriateCar.Id == car.Id))
                {
                    appropriateCar = car;
                    break;
                }
            }


            return appropriateCar;
        }
        private Car AppropriateCar(Models.Models.Location startingLocaion, RequestInfo request, Company company)
        {
            var nearBycars = data.Cars.Where(x => x.Company.Id == company.Id).Where(x => x.CarStatus == CarStatus.Free).Where(x => Math.Abs(x.Location.Latitude - startingLocaion.Latitude) <= 0.0150 && Math.Abs(x.Location.Longitude - startingLocaion.Longitude) <= 0.0150).ToList();
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
                if (!(data.CarsDismissedRequests.Where(x => x.Request.Id == request.Id).Any(x => x.Car.Id == car.Id)) && !(data.ActiveRequests.Any(x => x.AppropriateCar.Id == car.Id)))
                {
                    chosenCar = car;
                }
            }


            return chosenCar;
        }

        private void NotTakenRequest(int requestId)
        {
            var activeRequest = data.ActiveRequests.Where(x => x.Request.Id == requestId).Include(x => x.Request).Include(x => x.AppropriateCar).FirstOrDefault();
            TimeSpan diff = DateTime.Now - activeRequest.DateTimeChosenCar;
            if (diff.TotalSeconds > 15)
            {
                data.CarsDismissedRequests.Add(new CarDismissedRequest()
                {
                    Car = activeRequest.AppropriateCar,
                    Request = activeRequest.Request
                });
                data.SaveChanges();
                var newCar = AppropriateCar(activeRequest.Request.StartingLocation, activeRequest.Request, activeRequest.Request.Company);
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
        }

        private void NoCarChosen(int requestId)
        {
            var actReque = data.ActiveRequests.Where(x => x.Request.Id == requestId).Include(x => x.Request).FirstOrDefault();
            TimeSpan diff1 = DateTime.Now - actReque.DateTimeChosenCar;
            if (diff1.TotalSeconds > 90)
            {
                actReque.Request.RequestStatus = RequestStatusEnum.Dismissed;
            }
            else
            {
                var newCar = AppropriateCar(actReque.Request.StartingLocation, actReque.Request, actReque.Request.Company);
                if (newCar != null)
                {
                    actReque.AppropriateCar = newCar;
                    actReque.DateTimeChosenCar = DateTime.Now;
                    actReque.Request.RequestStatus = RequestStatusEnum.NotTaken;
                }
            }
        }

        public void AddDispatcherDashboard(DispatcherDashboard dashboard)
        {
            data.DispatchersDashboard.Add(dashboard);
            data.SaveChanges();
        }
    }

}
