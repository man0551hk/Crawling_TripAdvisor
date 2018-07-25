using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TripAdvisor_Crawling
{
    public class GoogleMapAPI
    {
        public List<Result> results { set; get; }
        public string status { set; get; }
    }

    public class Result
    {
        public List<Address_component> address_components { set; get; }
        public string formatted_address { set; get; }
        public Geometry geometry { set; get; }
        public string place_id { set; get; }
        public List<string> types { set; get; }
    }

    public class Address_component
    {
        public string long_name { set; get; }
        public string short_name { set; get; }
        public List<string> types { set; get; }
    }

    public class Geometry 
    {
        public Point Location { set; get; }
        public string location_type { set; get; }
        public Viewport viewport { set; get; }
    }

    public class Viewport
    {
        public Point northeast { set; get; }
        public Point southwest { set; get; }
    }

    public class Point
    {
        public double lat { set; get; }
        public double lng { set; get; }
    }
}
