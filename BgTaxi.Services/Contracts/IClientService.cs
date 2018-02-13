using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
   public interface IClientService
    {
        IEnumerable<Client> GetAll();
        IEnumerable<OnlineClient> GetAllOnlineClients();
        void AddClient(Client client);
        void RemoveClient(Client client);
        void AddOnlineClient(string userId, string connectionId);
        void RemoveOnlineClient(string connectionId);
        void UpdateRequestStatus(string userId);
    }
}
