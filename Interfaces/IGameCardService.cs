using BarcelonaGamesServer.Models;

namespace DotNetCardsServer.Interfaces
{
    public interface IGameCardService
    {
        IEnumerable<GameCard> GetAllCards();
        GameCard GetCardById(string id);
        GameCard AddCard(GameCard card);
    }
}
