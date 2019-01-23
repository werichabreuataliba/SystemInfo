using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SystemInfo.Controllers
{
    public class LocationInfo
    {
        public string ip { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public string country { get; set; }
        public string country_name { get; set; }
        public string postal { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timezone { get; set; }
        public string asn { get; set; }
        public string org { get; set; }
    }
}

