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

        public ClientService(IDatabase data)
        {
            _data = data;
        }
        public void AddClient(Client client)
        {
            _data.Clients.Add(client);
            _data.SaveChanges();
        }

        public void AddOnlineClient(string userId, string connectionId)
        {
            var client = _data.Clients.Where(x => x.UserId == userId).FirstOrDefault();
            if(client != null)
            {
                _data.OnlineClients.Add(new OnlineClient()
                {
                    Client = client,
                    ConnectionId = connectionId
                });
                _data.SaveChanges();
            }
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
