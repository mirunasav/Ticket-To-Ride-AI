using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class ClaimRouteMove : Move
    {
        public Model.GameBoard.Route Route {  get; set; }

        public TrainColor ColorUsed {  get; set; }

        public ClaimRouteMove(Game game, int playerIndex, TrainColor colorUsed, Model.GameBoard.Route route)
            : base(game, playerIndex)
        {
            this.Route = route;
            this.ColorUsed = colorUsed;
        }

        public override MakeMoveResponse Execute()
        {
            var player = Game.GetPlayer(playerIndex);

            //remove cards
            var removedCards = player.RemoveCards(ColorUsed, Route.Length);

            if (!removedCards)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.PlayerDoesNotHaveResources
                };
            }

            //remove trains from player
            player.RemainingTrains -= Route.Length;

            //add player points
            player.Points += Route.PointValue;

            //mark route as claimed
            Game.MarkRouteAsClaimed(Route, player);

            //change gameState
            Game.UpdateStateNextPlayerTurn();

            return new ClaimRouteResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasClaimedRoute,
                PlayerRemainingCards = Game.GetPlayer(playerIndex).Hand
            };
        }
    }
}
