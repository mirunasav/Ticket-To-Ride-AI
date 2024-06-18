using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Services;

namespace TicketToRide.Moves
{
    public class ClaimRouteMove : Move
    {
        public List<Model.GameBoard.Route> Route { get; set; } = new List<Model.GameBoard.Route>();

        public TrainColor ColorUsed { get; set; }

        public City Origin { get; set; }

        public City Destination { get; set; }

        public ClaimRouteMove(int playerIndex, TrainColor colorUsed, City origin, City destination)
            : base(playerIndex)
        {
            this.ColorUsed = colorUsed;
            this.Origin = origin;
            this.Destination = destination;
        }

        public ClaimRouteMove(int playerIndex, TrainColor colorUsed, List<Model.GameBoard.Route> route)
    : base(playerIndex)
        {
            this.ColorUsed = colorUsed;
            this.Origin = route.ElementAt(0).Origin;
            this.Destination = route.ElementAt(0).Destination;
            Route.AddRange(route);
        }

        public override MakeMoveResponse Execute(Game game)
        {
            var player = game.GetPlayer(PlayerIndex);

            var length = Route.ElementAt(0).Length;
            var pointValue = Route.ElementAt(0).PointValue;

            //remove cards
            (var canRemoveCards, var cardsToDiscard) = player.RemoveCards(ColorUsed, length);

            if (!canRemoveCards)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.PlayerDoesNotHaveResources
                };
            }

            //remove trains from player
            player.RemainingTrains -= length;

            //add player points
            player.Points += pointValue;

            //mark route as claimed
           var foundRoute = game.MarkRouteAsClaimed(Route.ElementAt(0).Origin, Route.ElementAt(0).Destination, player, ColorUsed);

            game.Board.DiscardPile.AddRange(cardsToDiscard);

            player.AddCompletedRoute(foundRoute);

            var newlyCompletedDestinations = player.GetNewlyCompletedDestinations();

            //change gameState
            game.UpdateStateNextPlayerTurn();

            var gameLogMessage = CreateGameLogMessage(
                game.Players.ElementAt(PlayerIndex).Name,
                Route.ElementAt(0).Origin.ToString(),
                Route.ElementAt(0).Destination.ToString());


            return new ClaimRouteResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasClaimedRoute,
                PlayerRemainingCards = game.GetPlayer(PlayerIndex).Hand,
                NewlyCompletedDestinations = newlyCompletedDestinations
            };
        }

        private string CreateGameLogMessage(string playerName, string originCity, string destinationCity)
        {
            return $"{playerName} has claimed a route from {originCity} to {destinationCity} using {ColorUsed}.";
        }

    }
}
