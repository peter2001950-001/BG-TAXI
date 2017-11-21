using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class OnlineDispatcher
    {
        [Key]
        public int Id { get; set; }
        public Dispatcher Dispatcher { get; set; }
        public string ConnectionId { get; set; }
    }
}
