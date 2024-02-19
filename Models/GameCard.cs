using System.ComponentModel.DataAnnotations;

namespace BarcelonaGamesServer.Models
{
    public class GameCard
    {
        public string ID { get; set; }

        [Required, StringLength(maximumLength: 256, MinimumLength = 2)]
        public string GameName { get; set; }

        [Required]
        public int BarcelonaScore { get; set; }
        [Required]
        public int RivalScore { get; set; }
        [Required]
        public List<string> BarcelonaSquad { get; set; }
        [Required]
        public List<string> RivalsSquad { get; set; }
        [Required]
        public List<string> WhoScore { get; set; }

        public string Link { get; set; }
        public string Image { get; set; }
        public string Where { get; set; }

    }
}
