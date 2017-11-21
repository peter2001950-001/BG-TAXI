using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
    public interface ICarService: IService
    {
        IEnumerable<Car> GetCars();
        Car GetCarByDriver(Driver driver);
        void UpdateCarInfo(Car car, Location location, bool absent, bool free);
        bool CarOnAddress(Car car);
        bool CarOnAddressCheck(Car car);
        void CreateCar(Car car);
        bool ModifyCar(int carId, Car modification);

        Car AppropriateCar(Models.Models.Location startingLocaion, RequestInfo request, Company company);
            Car AppropriateCar(Models.Models.Location startingLocaion, Company company);
    }
}
