using Microsoft.AspNetCore.Mvc;
using TicketToRide.Controllers.Requests;
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

            //move this in a game service thingy
            //var action = PlayerActions.DrawTrainCard;

            //var validateAction = game.ValidateAction(action, playerIndex);

            //if (!validateAction.IsValid)
            //{
            //    return BadRequest($"{validateAction.Message}");
            //}

            //game.Players.ElementAt(playerIndex).Act(action, game.Board);

            //return Json(new {
            //    boardDeck = game.Board.Deck,
            //    faceUpDeck = game.Board.FaceUpDeck,
            //    playerHand = game.Players[playerIndex].Hand
            //});
        }

        [HttpPost]
        public IActionResult ClaimRoute([FromBody] ClaimRouteRequest request)
        {
            //if (!Enum.IsDefined(typeof(City), request.Origin))
            //{
            //    return BadRequest("Origin city is invalid");
            //}

            //if (!Enum.IsDefined(typeof(City), request.Destination))
            //{
            //    return BadRequest("Destination city is invalid");
            //}

            //var originCity = (City)request.Origin;
            //var destinationCity = (City)request.Destination;

            ////var action = new ClaimRouteAction(parameters, board); action.act();
            ////var action = PlayerActions.DrawTrainCard;

            //var validateAction = game.ValidateClaimRouteAction(request.PlayerIndex,
            //    originCity,
            //    destinationCity,
            //    request.TrainColor,
            //    request.Length);

            //if (!validateAction.IsValid)
            //{
            //    return BadRequest($"{validateAction.Message}");
            //}

            //var player = game.Players.ElementAt(request.PlayerIndex);

            //var claimedRoute = game.Board.Routes.ClaimRoute(originCity,
            //    destinationCity,
            //    request.TrainColor,
            //    request.Length,
            //    player);

            //return Json(new
            //{
            //    ClaimedRoute = claimedRoute,
            //    Player = player
            //});            //if (!Enum.IsDefined(typeof(City), request.Origin))
            //{
            //    return BadRequest("Origin city is invalid");
            //}

            //if (!Enum.IsDefined(typeof(City), request.Destination))
            //{
            //    return BadRequest("Destination city is invalid");
            //}

            //var originCity = (City)request.Origin;
            //var destinationCity = (City)request.Destination;

            ////var action = new ClaimRouteAction(parameters, board); action.act();
            ////var action = PlayerActions.DrawTrainCard;

            //var validateAction = game.ValidateClaimRouteAction(request.PlayerIndex,
            //    originCity,
            //    destinationCity,
            //    request.TrainColor,
            //    request.Length);

            //if (!validateAction.IsValid)
            //{
            //    return BadRequest($"{validateAction.Message}");
            //}

            //var player = game.Players.ElementAt(request.PlayerIndex);

            //var claimedRoute = game.Board.Routes.ClaimRoute(originCity,
            //    destinationCity,
            //    request.TrainColor,
            //    request.Length,
            //    player);

            //return Json(new
            //{
            //    ClaimedRoute = claimedRoute,
            //    Player = player
            //});
            return Ok();
        }
    }
}
