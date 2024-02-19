using BarcelonaGamesServer.Models;
using DotNetCardsServer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;


namespace BarcelonaGamesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GameCardController:ControllerBase
    {
        private readonly IGameCardService _gameCardservice; // Assume this is an injected service handling business logic

        public GameCardController(IGameCardService cardService)
        {
            _gameCardservice = cardService;
        }

        // GET: api/cards
        [HttpGet]
        public IActionResult GetCards()
        {
            var cards = _gameCardservice.GetAllCards();
            return Ok(cards);
        }

        // POST: api/cards
        [HttpPost]
        public IActionResult CreateCard([FromBody] GameCard card)
        {
            var newCard = _gameCardservice.AddCard(card);
            return CreatedAtAction(nameof(GetCards), new { id = newCard.ID }, newCard);
        }

        // More actions (PUT, DELETE) as per your Express routes...
    }
}
