using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using TicketToRide.Controllers.Requests;
using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
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
            if (numberOfPlayers < 2 || numberOfPlayers > 4)
            {
                return BadRequest("Number of Players needs to be between 2 and 4");
            }

            var game = gameService.InitializeGame(numberOfPlayers);
            return Json(game);
        }

        [HttpGet]
        public IActionResult Index(int playerIndex)
        {
            //pasez player index si iau datele pt fiecare player, restul au hide data
            try
            {
                var game = gameService.GetGameInstance(playerIndex);

                if (game is null)
                {
                    return NotFound("Game is null");
                }

                return Json(game);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest("Invalid player index.");
            }

        }

        [HttpGet]
        public IActionResult GetGame()
        {

            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return NotFound("Game is null");
            }

            return Json(game);
        }


        [HttpDelete]
        public IActionResult Delete()
        {
            gameService.DeleteGame();

            return Ok();
        }

        [HttpGet]
        public IActionResult IsGameInitialized()
        {
            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return Json(false);
            }

            return Json(true);
        }

        [HttpGet]
        public IActionResult GetPlayers()
        {
            return Json(gameService.GetPlayers());
        }

        [HttpGet]
        public IActionResult DrawTrainCard(int playerIndex, int faceUpCardIndex = -1)
        {
            var response = gameService.DrawTrainCard(playerIndex, faceUpCardIndex);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            var game = gameService.GetGameInstance();
            return Json(new
            {
                Message = response.Message,
                playerHand = game.Players[playerIndex].Hand,
                faceUpDeck = game.Board.FaceUpDeck,
                gameState = game.GameState
            });
        }

        //click on 2 cities; see whether that route can be claimed
        [HttpGet]
        public IActionResult CanClaimRoute([FromQuery] int playerIndex, string origin, string destination)
        {
            bool originExists = Enum.TryParse(origin, out City originCity);
            bool destinationExists = Enum.TryParse(destination, out City destinationCity);

            if (!originExists || !destinationExists)
            {
                return BadRequest("Origin or destination city does not exist");
            }


            var response = gameService.CanClaimRoute(new CanClaimRouteRequest
            {
                PlayerIndex = playerIndex,
                Origin = originCity,
                Destination = destinationCity
            });

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is CanClaimRouteResponse canClaimRouteResponse)
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
            bool originExists = Enum.TryParse(request.OriginCity, out City originCity);
            bool destinationExists = Enum.TryParse(request.DestinationCity, out City destinationCity);
            bool trainColorExists = Enum.TryParse(request.ColorUsed, out TrainColor color);

            if (!originExists || !destinationExists)
            {
                return BadRequest("Origin or destination city does not exist");
            }

            if (!trainColorExists)
            {
                return BadRequest("This train color does not exist");
            }

            var response = gameService.ClaimRoute(request.PlayerIndex, originCity, destinationCity, color);

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

        [HttpGet]
        public IActionResult DrawDestinationCards(int playerIndex)
        {
            var response = gameService.DrawDestinationCards(playerIndex);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is DrawDestinationCardsResponse drawDestinationCardResponse)
            {
                return Json(new
                {
                    Message = drawDestinationCardResponse.Message,
                    DrawnDestinationCards = drawDestinationCardResponse.DrawnDestinationCards
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult DrawDestinationCards([FromBody] DrawDestinationCardsRequest request)
        {

            var response = gameService.ChooseDestinationCards(request);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is DrawDestinationCardsResponse drawDestinationCardResponse)
            {
                return Json(new
                {
                    Message = drawDestinationCardResponse.Message,
                    DrawnDestinationCards = drawDestinationCardResponse.DrawnDestinationCards
                });
            }
            else
            {
                return BadRequest();
            }
        }

        #region frontend
        [HttpGet]
        public IActionResult DoesRouteExist([FromQuery] string origin, string destination)
        {
            var result = gameService.CanRouteBeClaimed(origin, destination);
            return Json(result);
        }

        [HttpGet]
        public IActionResult GetGameState()
        {
            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return NotFound("Game is null");
            }

            return Json(new
            {
                gameState = game.GameState,
                playerTurn = game.PlayerTurn
            });
        }
        #endregion
    }
}
