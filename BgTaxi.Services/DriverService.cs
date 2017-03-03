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
        readonly IDatabase data;
        public DriverService(IDatabase data)
        {
            this.data = data;
        }

        public IEnumerable<Driver> GetAll()
        {
            return data.Drivers.AsEnumerable();

        }

        public Driver GetDriverByUserId(string userId)
        {
           return data.Drivers.Where(x => x.UserId == userId).FirstOrDefault();

        }

        public bool DriverModify(int Id, Driver modification)
        {
            var driver = data.Drivers.Where(x => x.Id == Id).FirstOrDefault();
            if(driver != null)
            {
                driver = modification;
                data.SaveChanges();
                return true;
            }
            return false;
        }

        public bool RemoveDriver(Driver driver)
        {
            data.Drivers.Remove(driver);
            data.SaveChanges();
            return true;
        }

        public void ChangeCarStatus(string driverId, CarStatus carStatus)
        {
            var driver = data.Drivers.Where(x => x.UserId == driverId).Include(x => x.Car).FirstOrDefault();
            driver.Car.CarStatus = carStatus;
            data.SaveChanges();
        }
        public void AddDriver(Driver driver)
        {
            data.Drivers.Add(driver);
            data.SaveChanges();
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }
    }
}
