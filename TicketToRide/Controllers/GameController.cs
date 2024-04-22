using Microsoft.AspNetCore.Mvc;
using TicketToRide.Controllers.Requests;
using TicketToRide.Controllers.Responses;
using TicketToRide.Services;

namespace TicketToRide.Controllers
{
    public class GameController : Controller
    {
        private readonly GameService gameService;

        public GameController(GameService gameService)
        {
            this.gameService = gameService;
        }

        [HttpGet]
        public IActionResult NewGame(int numberOfPlayers)
        {
            if(numberOfPlayers < 2 || numberOfPlayers > 4)
            {
                return BadRequest("Number of Players needs to be between 2 and 4");
            }

            var game = gameService.InitializeGame(numberOfPlayers);
            return Json(game);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var game = gameService.GetGameInstance();

            if(game is null)
            {
                return NotFound("Game is null");
            }

            return Json(gameService.GetGameInstance());
        }

        [HttpGet]
        public IActionResult GetPlayers()
        {
            return Json(gameService.GetPlayers());
        }
        [HttpGet]
        public IActionResult DrawCard(int playerIndex, int faceUpCardIndex = -1)
        {
            var response = gameService.DrawTrainCard(playerIndex, faceUpCardIndex);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            var game = gameService.GetGameInstance();
            return Json(new
            {
               Message =  response.Message,
               playerHand = game.Players[playerIndex].Hand,
               gameState = game.GameState
            });
        }

        //click on 2 cities; see whether that route can be claimed
        [HttpGet]
        public IActionResult CanClaimRoute([FromBody] CanClaimRouteRequest request)
        {
            var response = gameService.CanClaimRoute(request);

            if (!response.IsValid)
            {
                return BadRequest(response.Message); 
            }

            if(response is CanClaimRouteResponse canClaimRouteResponse)
            {
                return Json(new
                {
                    Message = canClaimRouteResponse.Message,
                    trainColors = canClaimRouteResponse.TrainColorsWhichCanBeUsed,
                    route = canClaimRouteResponse.Route
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult ClaimRoute([FromBody] ClaimRouteRequest request)
        {
            var response = gameService.ClaimRoute(request);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is ClaimRouteResponse claimRouteResponse)
            {
                return Json(new
                {
                    Message = claimRouteResponse.Message,
                    playerRemainingCards = claimRouteResponse.PlayerRemainingCards
                });
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
