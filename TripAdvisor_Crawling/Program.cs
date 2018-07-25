using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TripAdvisor_Crawling
{
    class Program
    {
        static void Main(string[] args)
        {
            Japan japan = new Japan();
            japan.Crawl();
        }
    }
}
