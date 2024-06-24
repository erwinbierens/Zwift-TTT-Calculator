using System;
using static System.Net.WebRequestMethods;

namespace WattsCalculator.Models
{
    public class Rider
    {
        public string? Name { get; set; }
        public int Height { get; set; }
        public double Weight { get; set; }
        public int FTP { get; set; }

        // Calculated properties
        public double PowerOutputWattsPerKg { get; set; }
        public double[] PullWattages { get; set; }
        public double[] PullWattagesPerKg { get; set; }
        public string[] PowerZones { get; set; }

        // Constructor to initialize arrays
        public Rider()
        {
            PullWattages = new double[6];
            PullWattagesPerKg = new double[6];
            PowerZones = new string[6];
        }
    }

}