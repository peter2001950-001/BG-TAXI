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

        public IEnumerable<Car> GetCars()
        {
            return data.Cars.AsEnumerable();

        }
        public Car GetCarByDriver(Driver driver)
        {
           Driver foundDriver = data.Drivers.Where(x => x.Id == driver.Id).Include(x => x.Car).FirstOrDefault();
            return foundDriver.Car;
        }
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

        public void CreateCar(Car car)
        {
            data.Cars.Add(car);
            data.SaveChanges();
        }

        public bool ModifyCar(int carId, Car modification)
        {
            var car = data.Cars.Where(x => x.Id == carId).FirstOrDefault();
            if(car != null)
            {
                car = modification;
                data.SaveChanges();
                return true;
            }
            return false;
        }
        
    }
}
