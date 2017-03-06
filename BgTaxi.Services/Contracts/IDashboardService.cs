using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
   public interface IDashboardService
    {

        IEnumerable<DispatcherDashboard> GetAll();
        void UpdateRequestStatus(string userId);
        object[] GetRequests(string userId);
        void AddDispatcherDashboard(DispatcherDashboard dashboard);
        Car AppropriateCar(Models.Models.Location startingLocaion, Company company);
    }

}
