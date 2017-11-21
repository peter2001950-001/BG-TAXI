using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.PlacesAPI.GoogleRequests;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
   public class CarService: BaseService, ICarService
    {
        private readonly IDatabase _data;
        public CarService(IDatabase data)
            :base(data)
        {
            this._data = data;
        }

        /// <summary>
        /// Returns All Cars
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Car> GetCars()
        {
            return _data.Cars.Include(x=>x.Company).AsEnumerable();

        }
        /// <summary>
        /// Returns the car of the driver or null if no car is found
        /// </summary>
        /// <param name="driver"></param>
        public Car GetCarByDriver(Driver driver)
        {
            Driver foundDriver = _data.Drivers.Where(x => x.Id == driver.Id).Include(x => x.Car).FirstOrDefault();
            return foundDriver?.Car;
        }

        /// <summary>
        /// Updates some basic information about the car
        /// </summary>
        /// <param name="car"></param>
        /// <param name="location"></param>
        /// <param name="absent"></param>
        /// <param name="free"></param>
        public void UpdateCarInfo(Car car, Models.Models.Location location, bool absent, bool free)
        {
            car.Location = location; 
            car.LastActiveDateTime = DateTime.Now;
            if (car.CarStatus == CarStatus.Offline)
            {
                if (_data.TakenRequests.Any(x => x.Car.Id == car.Id))
                {
                    car.CarStatus = CarStatus.Busy;
                }
                else
                {
                    car.CarStatus = CarStatus.Free;
                }
            }


            if (absent)
            {
                car.CarStatus = CarStatus.Absent;
            }
            if (free)
            {
                car.CarStatus = CarStatus.Free;
            }

            _data.SaveChanges();
        }

        public bool CarOnAddress(Car car)
        {
            var takenRequest = _data.TakenRequests.Where(x => x.Car.Id == car.Id).Include(x => x.Request).FirstOrDefault();
            if (takenRequest == null)
            {
                car.CarStatus = CarStatus.Free;
                _data.SaveChanges();
                return false;
            }
            else
            {
                takenRequest.Request.RequestStatus = RequestStatusEnum.OnAddress;
                takenRequest.OnAddressDateTime = DateTime.Now;
                _data.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Check if the car is near the address of the request
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        public bool CarOnAddressCheck(Car car)
        {
            if (car.CarStatus == CarStatus.Busy)
            {
                var takenRequest = _data.TakenRequests.Where(x => x.Car.Id == car.Id).Include(x => x.Request).FirstOrDefault();
                if (takenRequest == null)
                {
                    car.CarStatus = CarStatus.Free;
                    _data.SaveChanges();
                }
                else
                {
                    if (takenRequest.Request.RequestStatus == RequestStatusEnum.Taken)
                    {
                        var distance = DistanceBetweenTwoPoints.GetDistance(takenRequest.Request.StartingLocation, car.Location);
                        if (distance < 0.020)
                        {
                            takenRequest.Request.RequestStatus = RequestStatusEnum.OnAddress;
                            takenRequest.OnAddressDateTime = DateTime.Now;
                            _data.SaveChanges();
                            return true;
                        }
                    }
                }

                
            }
            return false;
        }

        /// <summary>
        /// Adds a new car
        /// </summary>
        /// <param name="car"></param>
        public void CreateCar(Car car)
        {
            _data.Cars.Add(car);
            _data.SaveChanges();
        }

        /// <summary>
        /// Changes the car returns null if no car is found with this carId
        /// </summary>
        /// <param name="carId"></param>
        /// <param name="modification"></param>
        /// <returns></returns>
        public bool ModifyCar(int carId, Car modification)
        {
            var car = _data.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null) return false;
            car = modification;
            _data.SaveChanges();
            return true;
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

            var sortedDistances = distances.OrderBy(x => x);
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
        /// <summary>
        /// Finds an appropriate car for the request location ignoring those who have danied it
        /// </summary>
        /// <param name="startingLocaion"></param>
        /// <param name="request"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public Car AppropriateCar(Models.Models.Location startingLocaion, RequestInfo request, Company company)
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

            var sortedDistances = distances.OrderBy(x => x);
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

    }
}
