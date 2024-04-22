using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Services
{
    public class MoveValidatorService
    {
        private readonly RouteService routeService;

        private readonly GameProvider gameProvider;

        public MoveValidatorService(GameProvider gameProvider, RouteService routeService)
        {
            this.gameProvider = gameProvider;
            this.routeService = routeService;
        }

        public MakeMoveResponse ValidateDrawTrainCardMove(DrawTrainCardMove drawTrainCardMove)
        {
            if ((drawTrainCardMove.faceUpCardIndex < 0 && drawTrainCardMove.faceUpCardIndex != -1) ||
               drawTrainCardMove.faceUpCardIndex >= drawTrainCardMove.Game.Board.FaceUpDeck.Count
               || (drawTrainCardMove.faceUpCardIndex != -1
               && !drawTrainCardMove.Game.Board.FaceUpDeck.ElementAt(drawTrainCardMove.faceUpCardIndex).IsAvailable))
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.InvalidFaceUpCardIndex
                };
            }

            if (drawTrainCardMove.Game.Board.Deck.Count == 0
                && drawTrainCardMove.Game.Board.FaceUpDeck.Count == 0
                && drawTrainCardMove.Game.Board.DiscardPile.Count == 0)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.NotEnoughTrainCardsToDrawFrom
                };
            }

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerDrawTrainCard
            };
        }

        public MakeMoveResponse ValidateClaimRouteMove (ClaimRouteMove claimRouteMove)
        {
            if(claimRouteMove.Route.Color != Model.Enums.TrainColor.Grey
                && claimRouteMove.ColorUsed != claimRouteMove.Route.Color)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.InvalidColorForRoute
                };
            }

            return routeService.CanPlayerClaimRoute(claimRouteMove);
        }

        public MakeMoveResponse CanRouteBeClaimed(CanClaimRouteMove canClaimRouteMove)
        {
            //check that the route exists
            canClaimRouteMove.Route = routeService.GetRoute(canClaimRouteMove.Origin, canClaimRouteMove.Destination);

            if (canClaimRouteMove.Route is null)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteDoesNotExist
                };
            }

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.Ok
            };
        }
        public void ValidateDrawDestinationCardMove()
        {

        }
    }
}
