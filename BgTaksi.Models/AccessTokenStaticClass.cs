using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models
{
   public static class AccessTokenStaticClass
    {
     
        public static string GenerateAccessToken(string oldAccessToken)
        {
            Models.Database db = new Models.Database();
           
            var oldAccTok = db.AccessTokens.Where(x => x.UniqueAccesToken == oldAccessToken).Include(x=>x.Device).FirstOrDefault();
            if (oldAccTok == null)
            {
                var previousAccTok = db.AccessTokens.Where(x => x.PreviousUniqueAccessToken == oldAccessToken).Include(x => x.Device).FirstOrDefault();
                if(previousAccTok== null)
                {
                    return null;
                }else
                {
                  
                    previousAccTok.Device.LastRequestDateTime = DateTime.Now;
                    db.SaveChanges();
                    return previousAccTok.UniqueAccesToken;
                }
            }

            var newAccessTokenString = Guid.NewGuid().ToString("D");
            oldAccTok.PreviousUniqueAccessToken = oldAccTok.UniqueAccesToken;
            oldAccTok.UniqueAccesToken = newAccessTokenString;
            oldAccTok.CreatedDateTime = DateTime.Now;
            oldAccTok.Device.LastRequestDateTime = DateTime.Now;
            db.SaveChanges();
            return newAccessTokenString;
        }
    }
}
