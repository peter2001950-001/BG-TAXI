using BgTaxi.GoogleRequests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BgTaxi.Web.GoogleRequests
{
    public static class GoogleAPIRequest
    {
        public static string GetAddress(double lat, double lon)
        {
            WebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "," + lon.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "&language=bg&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s");
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            RequestFormat flight = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestFormat>(responseFromServer);
            return flight.results[0].formatted_address;
        }
        public static DurationFormat GetDistance(BgTaxi.Models.Models.Location location1, BgTaxi.Models.Models.Location location2)
        {
            WebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + location1.Latitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + ", " + location1.Longitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + 
                "&destinations=" + location2.Latitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + ", " + location2.Longitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s&language=bg");
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            DurationFormat flight = Newtonsoft.Json.JsonConvert.DeserializeObject<DurationFormat>(responseFromServer);
            return flight;
        }
        public static Models.Models.Location GetLocation(string address)
        {
          address = address.Replace(" ", "+");
            WebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/json?address=" +address + "&language=bg&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s");
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            RequestFormat flight = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestFormat>(responseFromServer);
            if(flight.results.Count == 0)
            {
                return null;
            }
            var location = new Models.Models.Location()
            {
                Latitude = flight.results[0].geometry.location.lat,
                Longitude = flight.results[0].geometry.location.lng
            };

            return location;
        }

    }
}