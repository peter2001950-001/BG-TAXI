using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
    public interface IDeviceService: IService
    {
        bool AddDrvice(Device device);

        IEnumerable<Device> GetAll();
        Device GetDeviceByAccessToken(string accessToken);
    }
}
