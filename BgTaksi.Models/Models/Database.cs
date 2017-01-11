using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class Database:DbContext
    {
        public Database()
            :base("DefaultConnection")
        {

        }
        public DbSet<ActiveRequests> ActiveRequests { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<TakenRequests> TakenRequests { get; set; }
        public DbSet<RequestHistory> RequestHistory { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Dispatcher> Dispatchers { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<RequestInfo> RequestsInfo { get; set; }
        public DbSet<DispatcherDashboard> DispatchersDashboard { get; set; }
        public DbSet<CarDismissedRequest> CarsDismissedRequests { get; set; }


    }
}
