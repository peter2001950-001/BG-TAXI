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
        private readonly IDatabase data;
        public CarService(IDatabase data)
            :base(data)
        {
            this.data = data;
        }

        /// <summary>
        /// Returns All Cars
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Car> GetCars()
        {
            return data.Cars.Include(x=>x.Company).AsEnumerable();

        }
        /// <summary>
        /// Returns the car of the driver or null if no car is found
        /// </summary>
        /// <param name="driver"></param>
        public Car GetCarByDriver(Driver driver)
        {
            Driver foundDriver = data.Drivers.Where(x => x.Id == driver.Id).Include(x => x.Car).FirstOrDefault();
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
                if (data.TakenRequests.Any(x => x.Car.Id == car.Id))
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

            data.SaveChanges();
        }

        public bool CarOnAddress(Car car)
        {
            var takenRequest = data.TakenRequests.Where(x => x.Car.Id == car.Id).Include(x => x.Request).FirstOrDefault();
            if (takenRequest == null)
            {
                car.CarStatus = CarStatus.Free;
                data.SaveChanges();
                return false;
            }
            else
            {
                takenRequest.Request.RequestStatus = RequestStatusEnum.OnAddress;
                takenRequest.OnAddressDateTime = DateTime.Now;
                data.SaveChanges();
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
                var takenRequest = data.TakenRequests.Where(x => x.Car.Id == car.Id).Include(x => x.Request).FirstOrDefault();
                if (takenRequest == null)
                {
                    car.CarStatus = CarStatus.Free;
                    data.SaveChanges();
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
                            data.SaveChanges();
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
            data.Cars.Add(car);
            data.SaveChanges();
        }

        /// <summary>
        /// Changes the car returns null if no car is found with this carId
        /// </summary>
        /// <param name="carId"></param>
        /// <param name="modification"></param>
        /// <returns></returns>
        public bool ModifyCar(int carId, Car modification)
        {
            var car = data.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null) return false;
            car = modification;
            data.SaveChanges();
            return true;
        }
        
    }
}
