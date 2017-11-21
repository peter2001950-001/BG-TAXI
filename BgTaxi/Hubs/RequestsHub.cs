using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using BgTaxi.Services.Contracts;
using BgTaxi.PlacesAPI.GoogleRequests;
using BgTaxi.Models.Models;
using BgTaxi.Hubs.Models;
using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;
using BgTaxi.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BgTaxi.Hubs
{
    [HubName("RequestsHub")]
    public class RequestsHub : Hub
    {
        private IAccessTokenService _accessTokenService;
        private IDriverService _driverService;
        private ICarService _carService;
        private ICompanyService _companyService;
        private IRequestService _requestService;
        private IDispatcherService _dispatcherService;
        private IDashboardService _dashboardService;
        private IClientService _clientService;
        private readonly IDatabase _data;
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }
        public RequestsHub(IAccessTokenService accessTokenService, IDriverService driverService, ICarService carService, IDispatcherService dispatcherService, ICompanyService companyService, IRequestService requestService, IDashboardService dashboardService, IClientService clientService, IDatabase data)
        {
            _accessTokenService = accessTokenService;
            _carService = carService;
            _driverService = driverService;
            _dashboardService = dashboardService;
            _companyService = companyService;
            _requestService = requestService;
            _dispatcherService = dispatcherService;
            _clientService = clientService;
            _data = data;
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));

        }

        public void Pull(bool free = false, bool busy = false, bool absent = false, bool offline = false, bool offduty = false)
        {
            var userId = Context.User.Identity.GetUserId();

            var dashboard = _dashboardService.GetAll().Where(x => x.DispatcherUserId == userId).ToList();

            Clients.All.hey();
            // Remove those request which have been seen 
            foreach (var item in dashboard)
            {
                if (item.LastSeenStatus != RequestStatusEnum.NoCarChosen)
                {
                    var diff = DateTime.Now - item.LastSeen;
                    if (diff.TotalSeconds > 60)
                    {
                        var dispatcherDashboardRequest = dashboard.First(x => x.Id == item.Id);
                        _data.DispatchersDashboard.Remove(dispatcherDashboardRequest);
                    }
                }
            }

            //Try to find appropriate car for the rest of it
            var requestsList = _dashboardService.GetAll().Where(x => x.DispatcherUserId == userId).ToList();
            for (int i = 0; i < requestsList.Count; i++)
            {
                if (requestsList[i].Request != null)
                {
                    var reqId = requestsList[i].Request.Id;
                    if (requestsList[i].Request.RequestStatus == RequestStatusEnum.NotTaken)
                    {
                        NotTakenRequest(reqId);

                    }
                    else if (requestsList[i].Request.RequestStatus == RequestStatusEnum.NoCarChosen)
                    {
                        NoCarChosen(reqId);
                    }


                    var requestId = requestsList[i].Id;
                    var currentRequest = _dashboardService.GetAll().Where(x => x.Id == requestId).FirstOrDefault();
                    if (currentRequest != null && currentRequest.Request.RequestStatus != currentRequest.LastSeenStatus)
                    {
                        currentRequest.LastSeen = DateTime.Now;
                        currentRequest.LastSeenStatus = requestsList[i].Request.RequestStatus;
                    }
                }
            }


            var requests = _dashboardService.GetRequests(userId);

            var dispatcher = _dispatcherService.GetAll().First(x => x.UserId == userId);
            var dispatcherCompanyId = dispatcher.Company.Id;
            var cars = _carService.GetCars().Where(x => x.Company.Id == dispatcher.Company.Id).ToList();

            List<object> carsList = new List<object>();
            object[] carObjs = new object[cars.Count];
            int freeStatusCount = 0;
            int busyStatusCount = 0;
            int absentStatusCount = 0;
            int offlineStatusCount = 0;
            int offdutyStatusCount = 0;
            var addCar = false;
            for (int i = 0; i < cars.Count; i++)
            {
                addCar = false;
                TimeSpan diff = DateTime.Now - cars[i].LastActiveDateTime;
                if (diff.TotalMinutes > 2 && cars[i].CarStatus != CarStatus.OffDuty)
                {
                    cars[i].CarStatus = CarStatus.Offline;
                    _carService.SaveChanges();
                }
                switch (cars[i].CarStatus)
                {
                    case CarStatus.Free:
                        if (free)
                        {
                            addCar = true;
                        }
                        freeStatusCount++;
                        break;
                    case CarStatus.Busy:
                        if (busy)
                        {
                            addCar = true;
                        }
                        busyStatusCount++;
                        break;
                    case CarStatus.Absent:
                        if (absent)
                        {
                            addCar = true;
                        }
                        absentStatusCount++;
                        break;
                    case CarStatus.OffDuty:
                        if (offduty)
                        {
                            addCar = true;
                        }
                        offdutyStatusCount++;
                        break;
                    case CarStatus.Offline:
                        if (offline)
                        {
                            addCar = true;
                        }
                        offlineStatusCount++;
                        break;
                    default:
                        break;
                }

                if (addCar)
                {
                    carsList.Add(new { lng = cars[i].Location.Longitude, lat = cars[i].Location.Latitude, id = cars[i].InternalNumber, carStatus = cars[i].CarStatus });
                }
            }
            carObjs = carsList.ToArray();
            Clients.Client(Context.ConnectionId).pullRespond(new { requests = requests, cars = carObjs, freeStatusCount = freeStatusCount, busyStatusCount = busyStatusCount, absentStatusCount = absentStatusCount, offlineStatusCount = offlineStatusCount, offdutyStatusCount = offdutyStatusCount });
        }


        private void NotTakenRequest(int requestId)
        {
            var activeRequest = _requestService.GetActiveRequests().Where(x => x.Request.Id == requestId).FirstOrDefault();
            if (activeRequest?.Request != null)
            {
                var diff = DateTime.Now - activeRequest.DateTimeChosenCar;
                if (diff.TotalSeconds > 15)
                {
                    _data.CarsDismissedRequests.Add(new CarDismissedRequest()
                    {
                        Car = activeRequest.AppropriateCar,
                        Request = activeRequest.Request
                    });
                    _data.SaveChanges();
                    var driver = _driverService.GetDriverByCar(activeRequest.AppropriateCar);
                    var onlineDriver = _driverService.GetAllOnlineDrivers().Where(x => x.Driver.Id == driver.Id).FirstOrDefault();
                    if (onlineDriver!=null)
                    {
                        Clients.Client(onlineDriver.ConnectionId).RequestTimeout();
                    }
                  
                    var newCar = _carService.AppropriateCar(activeRequest.Request.StartingLocation, activeRequest.Request,
                        activeRequest.Request.Company);
                    if (newCar == null)
                    {
                        activeRequest.Request.RequestStatus = RequestStatusEnum.NoCarChosen;
                        activeRequest.AppropriateCar = null;
                        activeRequest.DateTimeChosenCar = DateTime.Now;
                    }
                    else
                    {
                        activeRequest.AppropriateCar = newCar;
                        activeRequest.DateTimeChosenCar = DateTime.Now;

                        driver = _driverService.GetDriverByCar(activeRequest.AppropriateCar);
                        onlineDriver = _driverService.GetAllOnlineDrivers().Where(x => x.Driver.Id == driver.Id).FirstOrDefault();
                        if (onlineDriver != null)
                        {
                            var distance = DistanceBetweenTwoPoints.GetDistance(activeRequest.Request.StartingLocation, activeRequest.AppropriateCar.Location);
                            Clients.Client(onlineDriver.ConnectionId).ReceiveRequest(new ReceivedRequest()
                            { 
                                Distance = distance, 
                                 FinishAddress = activeRequest.Request.FinishAddress,
                                  StartingAddress = activeRequest.Request.StartingAddress
                            }
                            );
                        }
                    }
                }
            }
            _data.SaveChanges();
        }
        private void NoCarChosen(int requestId)
        {
            var actReque = _requestService.GetActiveRequests().Where(x => x.Request.Id == requestId).FirstOrDefault();
            if (actReque?.Request != null)
            {
                var requestInfo = _requestService.GetRequestInfos().Where(x => x.Id == actReque.Request.Id).FirstOrDefault();
                if (requestInfo?.Company != null)
                {
                    var diff1 = DateTime.Now - actReque.DateTimeChosenCar;
                    if (diff1.TotalSeconds > 90)
                    {
                        actReque.Request.RequestStatus = RequestStatusEnum.Dismissed;
                        _requestService.RemoveActiveRequest(actReque);
                    }
                    else
                    {

                        var newCar = _carService.AppropriateCar(actReque.Request.StartingLocation, actReque.Request,
                            requestInfo.Company);
                        if (newCar != null)
                        {
                            actReque.AppropriateCar = newCar;
                            actReque.DateTimeChosenCar = DateTime.Now;
                            actReque.Request.RequestStatus = RequestStatusEnum.NotTaken;
                           var  driver = _driverService.GetDriverByCar(actReque.AppropriateCar);
                           var  onlineDriver = _driverService.GetAllOnlineDrivers().Where(x => x.Driver.Id == driver.Id).FirstOrDefault();
                            if (onlineDriver != null)
                            {
                                var distance = DistanceBetweenTwoPoints.GetDistance(actReque.Request.StartingLocation, actReque.AppropriateCar.Location);
                                Clients.Client(onlineDriver.ConnectionId).ReceiveRequest(new ReceivedRequest()
                                {
                                    Distance = distance,
                                    FinishAddress = actReque.Request.FinishAddress,
                                    StartingAddress = actReque.Request.StartingAddress
                                }
                                );
                            }
                        }
                    }
                }
                _data.SaveChanges();
            }
        }

        public override Task OnConnected()
        {
           
            if (Context.User.Identity.GetUserId() == null)
            {
                var userId = _accessTokenService.GetUserId(Context.QueryString["accessToken"]);
                if (userId != null)
                {
                    if (_driverService.GetAll().Where(x => x.UserId == userId).FirstOrDefault() != null)
                    {
                        _driverService.AddOnlineDriver(userId, Context.ConnectionId);

                    }
                    else if (_clientService.GetAll().Where(x => x.UserId == userId) != null)
                    {
                        _clientService.AddOnlineClient(userId, Context.ConnectionId);
                    }
                }
                else
                {
                    return base.OnDisconnected(true);
                }
            }
            else
            {
                _dispatcherService.AddOnlineDispatcher(Context.User.Identity.GetUserId(), Context.ConnectionId);

            }
            Clients.All.hey();
            return base.OnConnected();


        }
        public override Task OnDisconnected(bool stopCalled)
        {
            if (_driverService.GetAllOnlineDrivers().Where(x => x.ConnectionId == Context.ConnectionId) != null)
            {
                _driverService.RemoveOnlineDriver(Context.ConnectionId);
            } else if (_clientService.GetAllOnlineClients().Where(x => x.ConnectionId == Context.ConnectionId) != null)
            {
                _clientService.RemoveOnlineClient(Context.ConnectionId);
            } else if(_dispatcherService.GetAllOnlineDispatchers().Where(x=>x.ConnectionId == Context.ConnectionId) != null)
            {
                _dispatcherService.RemoveOnlineDispatcher(Context.ConnectionId);
            }
            return base.OnDisconnected(stopCalled);
        }

    }
    }