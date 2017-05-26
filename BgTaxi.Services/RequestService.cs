using BgTaxi.Models;
using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using BgTaxi.Services.Contracts;
using BgTaxi.PlacesAPI.GoogleRequests;

namespace BgTaxi.Services
{
    public class RequestService : IRequestService
    {
        private readonly IDatabase data;
        public RequestService(IDatabase data)
        {
            this.data = data;
        }

        /// <summary>
        /// Return all activeRequests
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ActiveRequest> GetActiveRequests()
        {
            return data.ActiveRequests.Include(x=>x.Request).AsEnumerable();
        }

        /// <summary>
        /// Returns all TakenRequests
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TakenRequest> GetTakenRequests()
        {
            return data.TakenRequests.AsEnumerable();
        }
        /// <summary>
        /// Returns all RequestsHistories
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RequestHistory> GetRequestHistories()
        {
            return data.RequestHistory.Include(x => x.Request).Include(x=>x.Car).AsEnumerable();
        }
        /// <summary>
        /// Returns all RequestInfos
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RequestInfo> GetRequestInfos()
        {
            return data.RequestsInfo.AsEnumerable();
        }
       /// <summary>
       /// Returns Active Request by its id
       /// </summary>
       /// <param name="id"></param>
       /// <returns></returns>
        public ActiveRequest GetActiveRequest(int id)
        {
            return data.ActiveRequests.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Returns Taken Request by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TakenRequest GetTakenRequest(int id)
        {
            return data.TakenRequests.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Returns RequestHistory by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RequestHistory GetRequestHistory(int id)
        {
            return data.RequestHistory.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Returns RequestInfo by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RequestInfo GetRequestInfo(int id)
        {
            return data.RequestsInfo.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Return the request which is chosen as appropriate for the car as an object ready to be parsed to JSON  
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        public object AppropriateRequest(Car car)
        {
            if (data.ActiveRequests.Any(x => x.AppropriateCar.Id == car.Id))
            {
                var request = data.ActiveRequests.Where(x => x.AppropriateCar.Id == car.Id).Include(x => x.Request).First();
                TimeSpan diff = DateTime.Now - request.DateTimeChosenCar;
                var distance = Math.Round(DistanceBetweenTwoPoints.GetDistance(request.Request.StartingLocation, new Models.Models.Location() { Latitude = car.Location.Latitude, Longitude = car.Location.Longitude }), 3);
                object requestObj = new { distance = distance, startAddress = request.Request.StartingAddress, finishAddress = request.Request.FinishAddress, id = request.Request.Id, time = 15 - Math.Round(diff.TotalSeconds, 0) };
                return requestObj;
            }
            return null;
        }

       /// <summary>
       /// Takes the answer of the driver and returns true if it is saved successfully otherwise false 
       /// </summary>
       /// <param name="answer"></param>
       /// <param name="requestId"></param>
       /// <param name="driver"></param>
       /// <returns></returns>
        public bool UpdateAnswer(bool answer, int requestId, Driver driver)
        {
            var driverCarInstance = data.Drivers.Where(x => x.Id == driver.Id).Include(x => x.Car).Include(x=>x.Company).First();
            if (answer == false)
            {
                var requestSelected = data.RequestsInfo.First(x => x.Id == requestId);
                requestSelected.RequestStatus = RequestStatusEnum.NoCarChosen;
                var request = data.ActiveRequests.FirstOrDefault(x => x.Request.Id == requestId);
                request.AppropriateCar = null;
                data.CarsDismissedRequests.Add(new CarDismissedRequest() { Car = driverCarInstance.Car, Request = requestSelected });
                data.SaveChanges();

                return true;
            }
            else
            {
                var request = data.ActiveRequests.Where(x => x.Request.Id == requestId).Include(x => x.Request).Include(x => x.AppropriateCar).FirstOrDefault();

                if (request != null && request.AppropriateCar == null)
                {
                    return false;
                }
                if (request.AppropriateCar.Id == driver.Car.Id)
                {
                    var distanceObject = GoogleAPIRequest.GetDistance(request.Request.StartingLocation, driver.Car.Location);

                    data.TakenRequests.Add(new TakenRequest()
                    {
                        Request = request.Request,
                        DateTimeTaken = DateTime.Now,
                        Car = driver.Car,
                        Company = driver.Company,
                        DriverLocation = new Models.Models.Location() { Latitude = driver.Car.Location.Latitude, Longitude = driver.Car.Location.Longitude },
                        DriverUserId = driver.UserId,
                        DuractionText = distanceObject.rows[0].elements[0].duration.text,
                        DuractionValue = distanceObject.rows[0].elements[0].duration.value,
                        DistanceText = distanceObject.rows[0].elements[0].distance.text,
                        DistanceValue = distanceObject.rows[0].elements[0].distance.value,
                        OnAddressDateTime = DateTime.Now
                    });

                    request.Request.RequestStatus = RequestStatusEnum.Taken;
                    request.AppropriateCar.CarStatus = CarStatus.Busy;
                    data.ActiveRequests.Remove(request);
                    data.SaveChanges();

                }
            }
            return true;
        }

        /// <summary>
        /// Mark the request as finished 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool FinishRequest(int requestId, string userId)
        {
            var request = data.TakenRequests.Where(x => x.Request.Id == requestId).Include(x=>x.Company).Include(x => x.Request).Include(x => x.Car).FirstOrDefault();
            if (request != null)
            {
                if (userId == request.DriverUserId)
                {
                    data.RequestHistory.Add(new RequestHistory()
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
                    data.TakenRequests.Remove(request);
                    data.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }

        /// <summary>
        /// Add a Request Info
        /// </summary>
        /// <param name="request"></param>
        public void AddRequestInfo(RequestInfo request)
        {
            data.RequestsInfo.Add(request);
            data.SaveChanges();
        }

        /// <summary>
        /// Add an Active Request
        /// </summary>
        /// <param name="request"></param>
        public void AddActiveRequest(ActiveRequest request)
        {
            data.ActiveRequests.Add(request);
            data.SaveChanges();
        }
    }
}

