using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BgTaxi.Web.GoogleRequests
{
    public static class DistanceBetweenTwoPoints
    {
        public static double GetDistance(BgTaxi.Models.Models.Location PointA, BgTaxi.Models.Models.Location PointB)
        {
            double lat1 = PointA.Latitude; double long1 = PointA.Longitude; double lat2 = PointB.Latitude;
            double long2 = PointB.Longitude;

            var d2r = Math.PI / 180;
            double dlong = (long2 - long1) * d2r;
            double dlat = (lat2 - lat1) * d2r;
            double a = Math.Pow(Math.Sin(dlat / 2.0), 2) + Math.Cos(lat1 * d2r) * Math.Cos(lat2 * d2r) * Math.Pow(Math.Sin(dlong / 2.0), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = 6367 * c;

            return d;
        }
    }
}