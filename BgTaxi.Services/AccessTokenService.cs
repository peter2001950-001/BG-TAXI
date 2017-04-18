using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IDatabase data;

        public AccessTokenService(IDatabase data)
        {
            this.data = data;
        }

        public string GetUserId(string accessToken)
        {
            var accessTokObj = this.data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).FirstOrDefault();
            if (accessTokObj == null)
            {
                return null;
            }
            return accessTokObj.Device.UserId;

        }
        public bool AddAccessToken(AccessToken accessToken)
        {
            this.data.AccessTokens.Add(accessToken);
            this.data.SaveChanges();
            return true;
        }


        public IEnumerable<AccessToken> GetAll()
        {
            return data.AccessTokens.AsEnumerable();
        }

        public bool IsUserLoggedIn(string accessToken)
        {
            var accTok = data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x=>x.Device).FirstOrDefault();
            if (accTok != null)
            {
                var device = data.Devices.FirstOrDefault(x => x.Id == accTok.Device.Id);
                if (device?.UserId != null)
                {
                    return true;
                }
            }
            return false;
            }
        public bool AddDeviceUserId(string accessToken, string userId)
        {
            
            var accTok = data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).First();
            if (accTok != null)
            {
                accTok.Device.UserId = userId;
                accTok.Device.LastRequestDateTime = DateTime.Now;
                data.SaveChanges();
                return true;
            }
            return false;
        }
        public string GenerateAccessToken(string oldAccessToken)
        {
            var oldAccTok = data.AccessTokens.Where(x => x.UniqueAccesToken == oldAccessToken).Include(x => x.Device).FirstOrDefault();
            if (oldAccTok == null)
            {
                var previousAccTok = data.AccessTokens.Where(x => x.PreviousUniqueAccessToken == oldAccessToken).Include(x => x.Device).FirstOrDefault();
                if (previousAccTok == null)
                {
                    return null;
                }
                else
                {

                    previousAccTok.Device.LastRequestDateTime = DateTime.Now;
                    data.SaveChanges();
                    return previousAccTok.UniqueAccesToken;
                }
            }

            var newAccessTokenString = Guid.NewGuid().ToString("D");
            oldAccTok.PreviousUniqueAccessToken = oldAccTok.UniqueAccesToken;
            oldAccTok.UniqueAccesToken = newAccessTokenString;
            oldAccTok.CreatedDateTime = DateTime.Now;
            oldAccTok.Device.LastRequestDateTime = DateTime.Now;
            data.SaveChanges();
            return newAccessTokenString;
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }
    }
}
