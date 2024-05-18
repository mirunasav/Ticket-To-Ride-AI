using System;
using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
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
                Message = ValidMovesMessages.ValidMove
            };
        }

        public MakeMoveResponse ValidateClaimRouteMove(ClaimRouteMove claimRouteMove)
        {
            claimRouteMove.Route = routeService.GetRoute(claimRouteMove.Origin, claimRouteMove.Destination);

            if (claimRouteMove.Route is null)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteDoesNotExist
                };
            }

            if (claimRouteMove.Route.Color != Model.Enums.TrainColor.Grey
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

            var isClaimed = canClaimRouteMove.Route.IsClaimed;

            if (isClaimed)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteAlreadyClaimed
                };
            }
            else
            {
                return new MakeMoveResponse
                {
                    IsValid = true,
                    Message = ValidMovesMessages.ValidMove
                };
            }
        }

        public MakeMoveResponse ValidateDrawDestinationCardsMove(DrawDestinationCardMove drawDestinationCardsMove)
        {
            var canDrawCards = drawDestinationCardsMove.Game.Board.DestinationCards.Count >= 3;

            if (canDrawCards)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.NotEnoughDestinationCards
                };
            }

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.ValidMove
            };
        }

        public MakeMoveResponse ValidateChooseDestinationCardsMove(ChooseDestinationCardMove chooseDestinationCardMove)
        {
            return new MakeMoveResponse { IsValid = false };
            //cheat: make move to try to post destination cards but others
            //mark the 3 destination cards as waitingToBeChosen.
            //check if the cards received are the ones waiting to be chosen
            //if yes, allow the move; at the end if any of the cards are returned to the deck,
            //unmark them
        }
    }
}
