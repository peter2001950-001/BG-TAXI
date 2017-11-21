using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BgTaxi.Hubs.Models
{
    public class ReceivedRequest
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public string StartingAddress { get; set; }
        public string FinishAddress { get; set; }
        
    }
}