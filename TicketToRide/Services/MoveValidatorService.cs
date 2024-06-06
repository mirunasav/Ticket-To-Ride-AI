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
            var game = gameProvider.GetGame();

            if ((drawTrainCardMove.faceUpCardIndex < 0 && drawTrainCardMove.faceUpCardIndex != -1) ||
               drawTrainCardMove.faceUpCardIndex >= game.Board.FaceUpDeck.Count
               || (drawTrainCardMove.faceUpCardIndex != -1
               && !game.Board.FaceUpDeck.ElementAt(drawTrainCardMove.faceUpCardIndex).IsAvailable))
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.InvalidFaceUpCardIndex
                };
            }

            if (drawTrainCardMove.faceUpCardIndex != -1
               && game.Board.FaceUpDeck.ElementAt(drawTrainCardMove.faceUpCardIndex).IsAvailable)
            {
                return new MakeMoveResponse
                {
                    IsValid = true,
                    Message = ValidMovesMessages.ValidMove
                };
            }

            if (game.Board.Deck.Where(c => c.IsAvailable).Count() + game.Board.DiscardPile.Count < 2)
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

            var possibleColors = claimRouteMove.Route.Select(r => r.Color).ToList();

            if (!possibleColors.Contains(Model.Enums.TrainColor.Grey)
                && !possibleColors.Contains(claimRouteMove.ColorUsed)
                && claimRouteMove.ColorUsed != TrainColor.Locomotive)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.InvalidColorForRoute
                };
            }

            var numberOfPlayers = gameProvider.GetNumberOfPlayers();

            return routeService.CanPlayerClaimRoute(claimRouteMove, numberOfPlayers);
        }

        public MakeMoveResponse CanRouteBeClaimed(CanClaimRouteMove canClaimRouteMove, int playerTrainsLeft = int.MaxValue)
        {
            //check that the route exists and is not occupied
            if(canClaimRouteMove.Route is null || canClaimRouteMove.Route.Count == 0)
            {
                canClaimRouteMove.Route = routeService.GetRoute(canClaimRouteMove.Origin, canClaimRouteMove.Destination);
            }

            if (canClaimRouteMove.Route is null || canClaimRouteMove.Route.Count == 0)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteDoesNotExist
                };
            }

            if(canClaimRouteMove.Route.ElementAt(0).Length > playerTrainsLeft)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.PlayerDoesNotHaveResources
                };
            }

            var numberOfPlayers = gameProvider.GetNumberOfPlayers();

            var isClaimed = routeService.IsRouteClaimed(canClaimRouteMove.Route, numberOfPlayers);

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
            var game = gameProvider.GetGame();

            var canDrawCards = game.Board.DestinationCards.Count >= 3;

            if (!canDrawCards)
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
            var cards = chooseDestinationCardMove.ChosenDestinationCards
                .Concat(chooseDestinationCardMove.NotChosenDestinationCards).ToList();

            foreach (var card in cards)
            {
                if (!card.IsWaitingToBeChosen)
                {
                    return new MakeMoveResponse
                    {
                        IsValid = false,
                        Message = InvalidMovesMessages.DestinationIsNotAnOption
                    };
                }
            }

            return new MakeMoveResponse { IsValid = true };
        }
    }
}
