using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json; 

namespace BgTaxi.PlacesAPI.GoogleRequests
{
    public static class GoogleAPIRequest
    {
        public static StreetAddress GetAddress(double lat, double lon)
        {
            var request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "," + lon.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "&language=bg&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s");
            var response = request.GetResponse();
            var dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            RequestFormat flight = JsonConvert.DeserializeObject<RequestFormat>(responseFromServer);
            return new StreetAddress(){ Street_address = flight.results[0].address_components[0].long_name, Street_number = flight.results[0].address_components[1].long_name, FormattedAddress = flight.results[0].formatted_address };
        }
        public static DurationFormat GetDistance(BgTaxi.Models.Models.Location location1, BgTaxi.Models.Models.Location location2)
        {
            WebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + location1.Latitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + ", " + location1.Longitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + 
                "&destinations=" + location2.Latitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + ", " + location2.Longitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s&language=bg");

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();
            DurationFormat flight = Newtonsoft.Json.JsonConvert.DeserializeObject<DurationFormat>(responseFromServer);
            return flight;
        }
        public static Models.Models.Location GetLocation(string address)
        {
          address = address.Replace(" ", "+");
            WebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/json?address=" +address + "&language=bg&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s");

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();
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

        public static List<Place> AutoCompleteList(string text, Models.Models.Location location, int radius)
        {
            text = text.Replace(" ", "+");
            WebRequest request =
                WebRequest.Create("https://maps.googleapis.com/maps/api/place/autocomplete/json?input=" + text +
                                  "&location=" + location.Latitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "," + location.Longitude.ToString("0.0000000", System.Globalization.CultureInfo.InvariantCulture) + "&radius="+radius+"&strictbounds&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s&language=bg");

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();
            var flight = Newtonsoft.Json.JsonConvert.DeserializeObject<AutoCompleteClasses>(responseFromServer);
            var placesList = new  List<Place>();
            if (flight.predictions.Count > 0)
            {
                foreach (var prediction in flight.predictions)
                {
                    placesList.Add(new Place()
                    {
                        MainText = prediction.structured_formatting.main_text,
                        PlaceId = prediction.place_id,
                        Address = prediction.structured_formatting.secondary_text
                    });
                }
            }
            return placesList;
        }

        public static  Place PlaceDetail(string placeId)
        {
            WebRequest request =
                WebRequest.Create(
                    "https://maps.googleapis.com/maps/api/place/details/xml?placeid=" + placeId +
                    "&key=AIzaSyCqFT1vjwghgVYc9Y_jbuD-ux10qQD9H0s&language=bg");

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();
            var place = new Place();
           XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseFromServer);
            
                place.Address = doc.SelectSingleNode("/PlaceDetailsResponse/result/formatted_address")?.InnerText;
                place.MainText = doc.SelectSingleNode("/PlaceDetailsResponse/result/name")?.InnerText;
            var lat = doc.SelectSingleNode("/PlaceDetailsResponse/result/geometry/location/lat")?.InnerText;
                var lon = doc.SelectSingleNode("/PlaceDetailsResponse/result/geometry/location/lng")?.InnerText; 
            if (lat != null && lon != null)
            {
                Models.Models.Location placeLocation = new Models.Models.Location()
                {
                    Longitude = Double.Parse(lon.Replace(".", ",")),
                    Latitude = Double.Parse(lat.Replace(".", ","))
                };
                place.Location = placeLocation;
                }
            return place;
        }

    }
}