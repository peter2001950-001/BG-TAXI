using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
   public class DriverService: IDriverService
    {
        readonly IDatabase _data;
        public DriverService(IDatabase data)
        {
            this._data = data;
        }

        /// <summary>
        /// Return all Drivers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Driver> GetAll()
        {
            return _data.Drivers.Include(x=>x.Car).Include(x=>x.Company).ToList();

        }

        /// <summary>
        /// Returns Driver by its user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Driver GetDriverByUserId(string userId)
        {
           return _data.Drivers.FirstOrDefault(x => x.UserId == userId);

        }
        /// <summary>
        /// Modify Driver's information
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="modification"></param>
        /// <returns></returns>
        public bool DriverModify(int Id, Driver modification)
        {
            var driver = _data.Drivers.FirstOrDefault(x => x.Id == Id);
            if(driver != null)
            {
                driver = modification;
                _data.SaveChanges();
                return true;
            }
            return false;
        }

        public void AddCar(int driverId,Car car)
        {
            var driver = _data.Drivers.Where(x => x.Id == driverId).FirstOrDefault();
            if (driver != null)
            {
                driver.Car = car;
                _data.SaveChanges();
               
            }
            else
            {
                throw new NullReferenceException();
            }
            
        }

        /// <summary>
        /// Delete the Driver
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public bool RemoveDriver(Driver driver)
        {
            _data.Drivers.Remove(driver);
            _data.SaveChanges();
            return true;
        }

        /// <summary>
        /// Sets a new car status of the driverID's car
        /// </summary>
        /// <param name="driverId"></param>
        /// <param name="carStatus"></param>
        public void ChangeCarStatus(string driverId, CarStatus carStatus)
        {
            var driver = _data.Drivers.Where(x => x.UserId == driverId).Include(x => x.Car).FirstOrDefault();
            driver.Car.CarStatus = carStatus;
            _data.SaveChanges();
        }
        /// <summary>
        /// Adds a Driver
        /// </summary>
        /// <param name="driver"></param>
        public void AddDriver(Driver driver)
        {
            _data.Drivers.Add(driver);
            _data.SaveChanges();
        }

        public void SaveChanges()
        {
            _data.SaveChanges();
        }
        public Driver GetDriverByCar(Car car)
        {
            var driver = _data.Drivers.Include(x => x.Car).Where(x => x.Car.Id == car.Id).FirstOrDefault();
            return driver;
        }

        public IEnumerable<OnlineDriver> GetAllOnlineDrivers()
        {
            return _data.OnlineDrivers.Include(x => x.Driver).AsEnumerable();
        }

        public void AddOnlineDriver(string userId, string connectionId)
        {
            var driver = _data.Drivers.Where(x => x.UserId == userId).FirstOrDefault();
            if (driver != null)
            {
                _data.OnlineDrivers.Add(new OnlineDriver()
                {
                    ConnectionId = connectionId,
                    Driver = driver
                });
                _data.SaveChanges();
            }
        }

        public void RemoveOnlineDriver(string connectionId)
        {
            var driver = _data.OnlineDrivers.Where(x => x.ConnectionId == connectionId).FirstOrDefault();
            if(driver != null)
            {
                _data.OnlineDrivers.Remove(driver);
                _data.SaveChanges();
            }
        }
    }
}
