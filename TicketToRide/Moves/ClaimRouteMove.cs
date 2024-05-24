using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class ClaimRouteMove : Move
    {
        public IList<Model.GameBoard.Route> Route {  get; set; }

        public TrainColor ColorUsed {  get; set; }

        public City Origin {  get; set; }

        public City Destination { get; set; }
        public ClaimRouteMove(Game game, int playerIndex, TrainColor colorUsed, City origin, City destination)
            : base(game, playerIndex)
        {
            this.ColorUsed = colorUsed;
            this.Origin = origin;
            this.Destination = destination;
        }

        public override MakeMoveResponse Execute()
        {
            var player = Game.GetPlayer(PlayerIndex);

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
            Game.MarkRouteAsClaimed(Route.ElementAt(0).Origin, Route.ElementAt(0).Destination, player, ColorUsed);

            player.AddCompletedRoute(Route.ElementAt(0));

            var newlyCompletedDestinations = player.GetNewlyCompletedDestinations();

            //change gameState
            Game.UpdateStateNextPlayerTurn();

            return new ClaimRouteResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasClaimedRoute,
                PlayerRemainingCards = Game.GetPlayer(PlayerIndex).Hand,
                NewlyCompletedDestinations = newlyCompletedDestinations
            };
        }
    }
}
