using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using BgTaxi.Models.Models;
using BgTaxi.Models;
using System.Data.Entity;
using BgTaxi.PlacesAPI.GoogleRequests;

namespace BgTaxi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDatabase _data;
        public DashboardService(IDatabase data)
        {
            this._data = data;
        }
        public IEnumerable<DispatcherDashboard> GetAll()
        {
            return _data.DispatchersDashboard.AsEnumerable();
        }

        public object[] GetRequests(string userId)
        {

            var requestsList = _data.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request).ToList();
            var requests = new object[requestsList.Count];

            for (var i = 0; i < requestsList.Count; i++)
            {
                string duraction = null;
                string carId = null;
                if (requestsList[i].Request.RequestStatus == RequestStatusEnum.Taken)
                {
                    var requesttId = requestsList[i].Request.Id;
                    var takenRequest = _data.TakenRequests.Where(x => x.Request.Id == requesttId).Include(x => x.Car).FirstOrDefault();

                    if (takenRequest != null)
                    {
                        duraction = takenRequest.DuractionText;
                        carId = takenRequest.Car.InternalNumber;
                    }
                }
                requests[i] = new { id = requestsList[i].Request.Id, startingAddress = requestsList[i].Request.StartingAddress, finishAddress = requestsList[i].Request.FinishAddress, requestStatus = requestsList[i].Request.RequestStatus, carId, duraction };
            }
            return requests;
        }

        public void UpdateRequestStatus(string userId)
        {
            var dashboard = _data.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request).ToList();
            foreach (var item in dashboard)
            {
                if (item.LastSeenStatus != RequestStatusEnum.NoCarChosen)
                {
                    var diff = DateTime.Now - item.LastSeen;
                    if (diff.TotalSeconds > 60)
                    {
                        var dispatcherDashboardRequest = dashboard.First(x => x.Id == item.Id);
                        _data.DispatchersDashboard.Remove(dispatcherDashboardRequest);
                        _data.SaveChanges();
                    }
                }

                if(item.Request.RequestStatus == RequestStatusEnum.NotTaken)
                {
                    var activeRequest = _data.ActiveRequests.Where(x => x.Request.Id == item.Request.Id).Include(x=>x.AppropriateCar).FirstOrDefault();
                    if (activeRequest != null)
                    {
                        TimeSpan timeSpan = DateTime.Now - activeRequest.DateTimeChosenCar;
                        if (timeSpan.TotalSeconds > 15)
                        {
                            var dismissedCar = activeRequest.AppropriateCar;
                            _data.CarsDismissedRequests.Add(new CarDismissedRequest() { Car = dismissedCar, Request = item.Request });
                            _data.SaveChanges();
                            activeRequest.AppropriateCar = null;
                            activeRequest.Request.RequestStatus = RequestStatusEnum.NoCarChosen;
                           
                            _data.SaveChanges();
                        }
                    }
                }
            }

            var requestsList = _data.DispatchersDashboard.Where(x => x.DispatcherUserId == userId).Include(x => x.Request).ToList();
            for (int i = 0; i < requestsList.Count; i++)
            {
                if (requestsList[i].Request != null)
                {
                    var reqId = requestsList[i].Request.Id;
                    if (requestsList[i].Request.RequestStatus == RequestStatusEnum.NotTaken)
                    {
                        NotTakenRequest(reqId);

                    }
                    else if (requestsList[i].Request.RequestStatus == RequestStatusEnum.NoCarChosen)
                    {
                        NoCarChosen(reqId);
                    }


                    var requestId = requestsList[i].Id;
                    var currentRequest = _data.DispatchersDashboard.Where(x => x.Id == requestId).Include(x => x.Request).FirstOrDefault();
                    if (currentRequest != null && currentRequest.Request.RequestStatus != currentRequest.LastSeenStatus)
                    {
                        currentRequest.LastSeen = DateTime.Now;
                        currentRequest.LastSeenStatus = requestsList[i].Request.RequestStatus;
                    }
                }
            }

            _data.SaveChanges();
        }

        public Car AppropriateCar(Models.Models.Location startingLocaion, Company company)
        {
            var nearBycars = _data.Cars.Where(x => x.Company.Id == company.Id).Where(x => x.CarStatus == CarStatus.Free).Where(x => Math.Abs(x.Location.Latitude - startingLocaion.Latitude) <= 0.0300 && Math.Abs(x.Location.Longitude - startingLocaion.Longitude) <= 0.0300).ToList();
            var distances = new List<double>();
            var dictionary = new Dictionary<double, Car>();
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

           var sortedDistances =  distances.OrderBy(x => x);
            foreach (var item in sortedDistances)
            {
                var car = dictionary[item];
                if (!_data.ActiveRequests.Any(x => x.AppropriateCar.Id == car.Id))
                {
                    appropriateCar = car;
                    break;
                }
            }


            return appropriateCar;
        }
        private Car AppropriateCar(Models.Models.Location startingLocaion, RequestInfo request, Company company)
        {
            var nearBycars = _data.Cars.Where(x => x.Company.Id == company.Id).Where(x => x.CarStatus == CarStatus.Free).Where(x => Math.Abs(x.Location.Latitude - startingLocaion.Latitude) <= 0.0150 && Math.Abs(x.Location.Longitude - startingLocaion.Longitude) <= 0.0150).ToList();
            var distances = new List<double>();
            var dictionary = new Dictionary<double, Car>();
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

           var sortedDistances =  distances.OrderBy(x => x);
            foreach (var item in sortedDistances)
            {
                var car = dictionary[item];
                if (!(_data.CarsDismissedRequests.Where(x => x.Request.Id == request.Id).Any(x => x.Car.Id == car.Id)) && !(_data.ActiveRequests.Any(x => x.AppropriateCar.Id == car.Id)))
                {
                    chosenCar = car;
                }
            }


            return chosenCar;
        }

        private void NotTakenRequest(int requestId)
        {
            var activeRequest = _data.ActiveRequests.Where(x => x.Request.Id == requestId).Include(x => x.Request).Include(x => x.AppropriateCar).FirstOrDefault();
            if (activeRequest?.Request != null)
            {
                var diff = DateTime.Now - activeRequest.DateTimeChosenCar;
                if (diff.TotalSeconds > 15)
                {
                    _data.CarsDismissedRequests.Add(new CarDismissedRequest()
                    {
                        Car = activeRequest.AppropriateCar,
                        Request = activeRequest.Request
                    });
                    _data.SaveChanges();
                    var newCar = AppropriateCar(activeRequest.Request.StartingLocation, activeRequest.Request,
                        activeRequest.Request.Company);
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
            _data.SaveChanges();
        }

        private void NoCarChosen(int requestId)
        {
            var actReque = _data.ActiveRequests.Where(x => x.Request.Id == requestId).Include(x => x.Request).FirstOrDefault();
            if (actReque?.Request != null)
            {
                var requestInfo = _data.RequestsInfo.Where(x => x.Id == actReque.Request.Id).Include(x => x.Company).FirstOrDefault();
                if (requestInfo?.Company != null)
                {
                    var diff1 = DateTime.Now - actReque.DateTimeChosenCar;
                    if (diff1.TotalSeconds > 90)
                    {
                        actReque.Request.RequestStatus = RequestStatusEnum.Dismissed;
                        _data.ActiveRequests.Remove(actReque);
                    }
                    else
                    {

                        var newCar = AppropriateCar(actReque.Request.StartingLocation, actReque.Request,
                            requestInfo.Company);
                        if (newCar != null)
                        {
                            actReque.AppropriateCar = newCar;
                            actReque.DateTimeChosenCar = DateTime.Now;
                            actReque.Request.RequestStatus = RequestStatusEnum.NotTaken;
                        }
                    }
                }
                _data.SaveChanges();
            }
        }

        public void AddDispatcherDashboard(DispatcherDashboard dashboard)
        {
            _data.DispatchersDashboard.Add(dashboard);
            _data.SaveChanges();
        }
    }

}