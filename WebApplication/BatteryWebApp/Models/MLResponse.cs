using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace BatteryWebApp.Models
{
    public class MLResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public DateTime dateStamp { get; set; }
        public string Response { get; set; }
    }
}