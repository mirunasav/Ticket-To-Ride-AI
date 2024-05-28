using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class ClaimRouteMove : Move
    {
        public IList<Model.GameBoard.Route> Route { get; set; } = new List<Model.GameBoard.Route>();

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

        public ClaimRouteMove(int playerIndex, TrainColor colorUsed, Model.GameBoard.Route route)
    : base(playerIndex)
        {
            this.ColorUsed = colorUsed;
            this.Origin = route.Origin;
            this.Destination = route.Destination;
            Route.Add(route);
        }


        public override MakeMoveResponse Execute(Game game)
        {
            var player = game.GetPlayer(PlayerIndex);

            var length = Route.ElementAt(0).Length;
            var pointValue = Route.ElementAt(0).PointValue;

            //remove cards
            var removedCards = player.RemoveCards(ColorUsed, length);

            if (!removedCards)
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
            game.MarkRouteAsClaimed(Route.ElementAt(0).Origin, Route.ElementAt(0).Destination, player, ColorUsed);

            player.AddCompletedRoute(Route.Where(r =>
            r.Color == ColorUsed
            || r.Color == TrainColor.Grey
            || ColorUsed == TrainColor.Locomotive)
                .First());

            var newlyCompletedDestinations = player.GetNewlyCompletedDestinations();

            //change gameState
            game.UpdateStateNextPlayerTurn();

            return new ClaimRouteResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasClaimedRoute,
                PlayerRemainingCards = game.GetPlayer(PlayerIndex).Hand,
                NewlyCompletedDestinations = newlyCompletedDestinations
            };
        }
    }
}
