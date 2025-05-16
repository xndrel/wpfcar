using System;

namespace WpfApp1
{
    public class Car
    {
        public int CarID { get; set; }
        public int CarDetailID { get; set; }
        public string LicensePlate { get; set; }
        public string Color { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsAvailable { get; set; }
        public decimal PricePerHour { get; set; }

        // Информация из CarDetails
        public string Type { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal EngineVolume { get; set; }
        public string FuelType { get; set; }
    }
} 