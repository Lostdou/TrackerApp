namespace TrackerAPI.Data
{
    public class UserLocation
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string PairingCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}