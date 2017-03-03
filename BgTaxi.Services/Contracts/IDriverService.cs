using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
   public interface IDriverService: IService
    {
        Driver GetDriverByUserId(string userId);
        IEnumerable<Driver> GetAll();
        bool DriverModify(int Id, Driver modification);
        bool RemoveDriver(Driver driver);
        void ChangeCarStatus(string driverId, CarStatus carStatus);
        void AddDriver(Driver driver);
    }
}
