using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TripAdvisor_Crawling
{
    public class location
    {
        public string url { set; get; }
        public string name_en { set; get; }
        public string name_tc { set; get; }
        public int type_id { set; get; }
        public string country_en { set; get; }
        public string country_tc { set; get; }
        public string area_tc { set; get; }
        public string area_en { set; get; }
        public string district_tc { set; get; }
        public string district_en { set; get; }
        public string address_tc { set; get; }
        public string address_en { set; get; }
        public string extend_address { set; get; }
        public string postal_code { set; get; }
        public string phone  { set; get; }
        public string website { set; get; }
        public List<string> imageList { set; get; }
    }
}
