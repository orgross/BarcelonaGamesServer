namespace BarcelonaGamesServer.Models.User
{
    public class User
    {
        public string Id { get; set; }
        public Name Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public Image Image { get; set; }
        public Address Address { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsBusiness { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
