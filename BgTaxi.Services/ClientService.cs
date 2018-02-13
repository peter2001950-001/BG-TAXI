using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BgTaxi.Models.Models;
using BgTaxi.Models;
using System.Data.Entity;

namespace BgTaxi.Services
{
    public class ClientService : IClientService
    {
        private readonly IDatabase _data;
        private readonly ICarService _carService;
        public ClientService(IDatabase data, ICarService carService)
        {
            _data = data;
            _carService = carService;
        }
        public void AddClient(Client client)
        {
            _data.Clients.Add(client);
            _data.SaveChanges();
        }

        public void AddOnlineClient(string userId, string connectionId)
        {
            var client = _data.Clients.Where(x => x.UserId == userId).FirstOrDefault();
            if (client != null)
            {
                _data.OnlineClients.Add(new OnlineClient()
                {
                    Client = client,
                    ConnectionId = connectionId
                });
                _data.SaveChanges();
            }
        }

        public void UpdateRequestStatus(string userId)
        {
            var client = _data.Clients.Where(c => c.UserId == userId).FirstOrDefault();
            if (client != null)
            {
                var clientRequest = _data.ClientRequests.Where(x => x.Client.Id == client.Id).Include(x => x.Request).FirstOrDefault();
                if (clientRequest != null)
                {
                    var activeRequest = _data.ActiveRequests.Where(x => x.Request.Id == clientRequest.Request.Id).Include(x=>x.AppropriateCar).FirstOrDefault();
                    if (activeRequest != null)
                    {
                        if (clientRequest.Request.RequestStatus == RequestStatusEnum.NotTaken)
                        {
                            
                            TimeSpan timeSpan = DateTime.Now - activeRequest.DateTimeChosenCar;
                            if (timeSpan.TotalSeconds > 15)
                            {
                                var dismissedCar = activeRequest.AppropriateCar;
                                _data.CarsDismissedRequests.Add(new CarDismissedRequest() { Car = dismissedCar, Request = clientRequest.Request });
                                _data.SaveChanges();
                                var newCar = _carService.AppropriateCar(clientRequest.Request.StartingLocation, clientRequest.Request);
                                if (newCar == null)
                                {
                                    clientRequest.Request.RequestStatus = RequestStatusEnum.NoCarChosen;
                                    activeRequest.AppropriateCar = null;
                                    activeRequest.DateTimeChosenCar = DateTime.Now;
                                }
                                else
                                {
                                    activeRequest.AppropriateCar = newCar;
                                    activeRequest.DateTimeChosenCar = DateTime.Now;
                                }
                                _data.SaveChanges();
                            }
                        }
                        else if (clientRequest.Request.RequestStatus == RequestStatusEnum.NoCarChosen)
                        {
                            var diff1 = DateTime.Now - activeRequest.DateTimeChosenCar;
                            if (diff1.TotalSeconds > 90)
                            {
                                clientRequest.Request.RequestStatus = RequestStatusEnum.Dismissed;
                                _data.ActiveRequests.Remove(activeRequest);
                            }
                            else
                            {

                                var newCar = _carService.AppropriateCar(clientRequest.Request.StartingLocation, clientRequest.Request);
                                if (newCar != null)
                                {
                                    activeRequest.AppropriateCar = newCar;
                                    activeRequest.DateTimeChosenCar = DateTime.Now;
                                    clientRequest.Request.RequestStatus = RequestStatusEnum.NotTaken;
                                }
                            }
                        }
                        _data.SaveChanges();
                    }
                }

            }
            _data.SaveChanges();
        }

        public IEnumerable<Client> GetAll()
        {
            return _data.Clients.AsEnumerable();
        }

        public IEnumerable<OnlineClient> GetAllOnlineClients()
        {
            return _data.OnlineClients.Include(x => x.Client).AsEnumerable();
        }

        public void RemoveClient(Client client)
        {
            _data.Clients.Remove(client);
            _data.SaveChanges();
        }

        public void RemoveOnlineClient(string connectionId)
        {
            var client = _data.OnlineClients.Include(x => x.ConnectionId == connectionId).FirstOrDefault();
            if (client != null)
            {
                _data.OnlineClients.Remove(client);
                _data.SaveChanges();
            }
        }
    }
}
