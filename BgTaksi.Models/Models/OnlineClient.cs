using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class OnlineClient
    {
        [Key]
        public int Id { get; set; }
        public Client Client { get; set; }
        public string ConnectionId { get; set; }
    }
}
