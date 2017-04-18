using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BgTaxi.Models.Models;

namespace BgTaxi.PlacesAPI
{
    public class Place
    {
        public string PlaceId { get; set; }
        public string MainText { get; set; }

        public string Address { get; set; }
        public Location Location { get; set; }


    }
}
