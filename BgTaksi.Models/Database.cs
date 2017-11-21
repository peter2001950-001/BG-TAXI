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
            this.Configuration.ProxyCreationEnabled = false;
        }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
        public DbSet<ActiveRequest> ActiveRequests { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<TakenRequest> TakenRequests { get; set; }
        public DbSet<RequestHistory> RequestHistory { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Dispatcher> Dispatchers { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<RequestInfo> RequestsInfo { get; set; }
        public DbSet<DispatcherDashboard> DispatchersDashboard { get; set; }
        public DbSet<CarDismissedRequest> CarsDismissedRequests { get; set; }

        public DbSet<OnlineDriver> OnlineDrivers
        {
            get; set;
        }

        public DbSet<OnlineDispatcher> OnlineDispatchers
        {
            get; set;
        }

        public DbSet<OnlineClient> OnlineClients
        {
            get; set;
        }

        public DbSet<Client> Clients
        {
            get; set;
        }
    }
}
