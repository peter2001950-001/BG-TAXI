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

        public IEnumerable<Driver> GetAll()
        {
            return _data.Drivers.AsEnumerable();

        }

        public Driver GetDriverByUserId(string userId)
        {
           return _data.Drivers.FirstOrDefault(x => x.UserId == userId);

        }

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

        public bool RemoveDriver(Driver driver)
        {
            _data.Drivers.Remove(driver);
            _data.SaveChanges();
            return true;
        }

        public void ChangeCarStatus(string driverId, CarStatus carStatus)
        {
            var driver = _data.Drivers.Where(x => x.UserId == driverId).Include(x => x.Car).FirstOrDefault();
            driver.Car.CarStatus = carStatus;
            _data.SaveChanges();
        }
        public void AddDriver(Driver driver)
        {
            _data.Drivers.Add(driver);
            _data.SaveChanges();
        }

        public void SaveChanges()
        {
            _data.SaveChanges();
        }
    }
}
