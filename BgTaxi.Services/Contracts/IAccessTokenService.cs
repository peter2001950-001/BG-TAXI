using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
    public  interface IAccessTokenService: IService
    {
        string GetUserId(string accessToken);
        bool AddAccessToken(AccessToken accessToken);

        IEnumerable<AccessToken> GetAll();
        bool IsUserLoggedIn(string accessToken);
        bool AddDeviceUserId(string accessToken, string userId);
        string GenerateAccessToken(string oldAccessToken);
    }
}
