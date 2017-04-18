using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class Database:DbContext, IDatabase
    {
        public Database()
            :base("DefaultConnection")
        {

        }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
        public virtual DbSet<ActiveRequest> ActiveRequests { get; set; }
        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<TakenRequest> TakenRequests { get; set; }
        public virtual DbSet<RequestHistory> RequestHistory { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Driver> Drivers { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<Dispatcher> Dispatchers { get; set; }
        public virtual DbSet<AccessToken> AccessTokens { get; set; }
        public virtual DbSet<RequestInfo> RequestsInfo { get; set; }
        public virtual DbSet<DispatcherDashboard> DispatchersDashboard { get; set; }
        public virtual DbSet<CarDismissedRequest> CarsDismissedRequests { get; set; }


    }
}
