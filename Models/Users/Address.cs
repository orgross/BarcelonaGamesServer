namespace BarcelonaGamesServer.Models.User
{
    public class Address
    {
        public string State { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public int HouseNumber { get; set; }
        public int Zip { get; set; } = 0;
    }
}
