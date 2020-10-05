using System;

namespace FitFileTools.Tools
{
    public class Record
    {
        public bool IsValid => true;

        public double? Elevation { get; set; }
        public DateTime Timestamp { get; set; }
        public double? TotalDistance { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}