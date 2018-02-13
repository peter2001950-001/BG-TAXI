using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models
{
    public interface IDatabase
    {
        DbSet<ActiveRequest> ActiveRequests
        {
            get;
        }
        DbSet<Car> Cars
        {
            get;
        }
        DbSet<TakenRequest> TakenRequests
        {
            get;
        }
        DbSet<RequestHistory> RequestHistory
        {
            get;
        }
        DbSet<Company> Companies
        {
            get;
        }
        DbSet<Driver> Drivers
        {
            get;
        }
        DbSet<Device> Devices
        {
            get;
        }
        DbSet<Dispatcher> Dispatchers
        {
            get;
        }
        DbSet<AccessToken> AccessTokens
        {
            get;
        }
        DbSet<RequestInfo> RequestsInfo
        {
            get;
        }
        DbSet<DispatcherDashboard> DispatchersDashboard
        {
            get;
        }
        DbSet<CarDismissedRequest> CarsDismissedRequests
        {
            get;
        }
        DbSet<OnlineDriver> OnlineDrivers
        {
            get;
        }
        DbSet<OnlineDispatcher> OnlineDispatchers
        {
            get;
        }
        DbSet<OnlineClient> OnlineClients
        {
            get;
        }
        DbSet<Client> Clients
        {
            get;
        }
        DbSet<ClientRequest> ClientRequests
        {
            get;
        }
        int SaveChanges();
    }
}
