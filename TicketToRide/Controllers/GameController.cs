using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using TicketToRide.Controllers.Requests;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Controllers
{
    public class GameController : Controller
    {
        public static Game game;

        [HttpGet]
        public IActionResult NewGame()
        {
            game = GameInitializer.InitializeGame(2);
            //HttpContext.Session.Set<Game>("Game", game);
            return Json(game);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Json(game);
        }

        [HttpGet]
        public IActionResult DrawCard(int playerIndex)
        {
            if (game == null)
            {
                return BadRequest("Initialize game first");
            }

            if (game.Players.Count < playerIndex || playerIndex < 0)
            {
                return BadRequest("Player index invalid");
            }

            //move this in a game service thingy
            var action = PlayerActions.DrawTrainCard;

            var validateAction = game.ValidateAction(action, playerIndex);

            if (!validateAction.IsValid)
            {
                return BadRequest($"{validateAction.Message}");
            }

            game.Players.ElementAt(playerIndex).Act(action, game.Board);

            return Json(new {
                boardDeck = game.Board.Deck,
                faceUpDeck = game.Board.FaceUpDeck,
                playerHand = game.Players[playerIndex].Hand
            });
        }

        [HttpPost]
        public IActionResult ClaimRoute([FromBody] ClaimRouteRequest request)
        {
            if (!Enum.IsDefined(typeof(City), request.Origin))
            {
                return BadRequest("Origin city is invalid");
            }

            if (!Enum.IsDefined(typeof(City), request.Destination))
            {
                return BadRequest("Destination city is invalid");
            }

            var originCity = (City)request.Origin;
            var destinationCity = (City)request.Destination;

            //var action = new ClaimRouteAction(parameters, board); action.act();
            //var action = PlayerActions.DrawTrainCard;

            var validateAction = game.ValidateClaimRouteAction(request.PlayerIndex,
                originCity,
                destinationCity,
                request.TrainColor,
                request.Length);

            if (!validateAction.IsValid)
            {
                return BadRequest($"{validateAction.Message}");
            }

            var player = game.Players.ElementAt(request.PlayerIndex);

            var claimedRoute = game.Board.Routes.ClaimRoute(originCity,
                destinationCity,
                request.TrainColor,
                request.Length,
                player);

            return Json(new
            {
                ClaimedRoute = claimedRoute,
                Player = player
            });
        }
    }
}
