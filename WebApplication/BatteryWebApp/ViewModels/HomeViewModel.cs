using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BatteryWebApp.Models;
using System.Net;

namespace BatteryWebApp.ViewModels
{
    public class HomeViewModel
    {
        public MLResponse Response { get; set; }

        public List<ResponseOutput> chartData { get; set; }
    }
}