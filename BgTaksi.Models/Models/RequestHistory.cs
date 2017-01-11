using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
   public class RequestHistory
    {
        [Key]
        public int Id { get; set; }
        public RequestInfo Request { get; set; }
        public DateTime DateTimeTaken { get; set; }
        public DateTime DateTimeFinished { get; set; }
        public Car Car { get; set; }
        public string DriverUserId { get; set; }
        public Company Company { get; set; }
    }
}
