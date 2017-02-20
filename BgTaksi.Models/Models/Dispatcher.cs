﻿using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class Dispatcher
    {
        [Key]
        public int Id { get; set; }
        public string  UserId { get; set; }
        public Company  Company { get; set; }
    }
}