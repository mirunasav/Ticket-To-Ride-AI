﻿using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class DrawTrainCard : Move
    {
        private int faceUpCardIndex = 0;
        public DrawTrainCard(Game game, int playerIndex, int faceUpCardIndex) : base(game, playerIndex)
        {
            this.faceUpCardIndex = faceUpCardIndex;
        }

        public override MakeMoveResponse ValidateMove()
        {
            //check validity of face up index: to be in bounds + available
            if ((faceUpCardIndex < 0 && faceUpCardIndex != -1) ||
                faceUpCardIndex >= Game.Board.FaceUpDeck.Count
                || (faceUpCardIndex != -1 && !Game.Board.FaceUpDeck.ElementAt(faceUpCardIndex).IsAvailable)) {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.InvalidFaceUpCardIndex
                };
            }

            if (Game.Board.Deck.Count == 0
                && Game.Board.FaceUpDeck.Count == 0
                && Game.Board.DiscardPile.Count == 0)
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

        public override MakeMoveResponse Execute()
        {
            bool isTurnFinishedByLocomotiveDraw = false;

            if (faceUpCardIndex != -1)
            {
                //draw from face up card
                var card = Game.Board.FaceUpDeck.ElementAt(faceUpCardIndex);
                Game.Board.FaceUpDeck.Remove(card);

                //add to player hand
                Game.Players.ElementAt(playerIndex).Hand.Add(card);

                //refill faceup deck
                Game.Board.PopulateFaceUpDeck();

                //if the drawn card was a locomotive, will change state to finished state
                isTurnFinishedByLocomotiveDraw = card.Color == TrainColor.Locomotive;
            }

            //choose to draw from deck
            else
            {
                //take first card from deck
                var card = Game.Board.Deck.Pop(1);

                //add to player hand
                Game.Players.ElementAt(playerIndex).Hand.AddRange(card);
            }

            var newGameState = UpdateGameState(isTurnFinished: isTurnFinishedByLocomotiveDraw);

            //if the player still has to draw cards, make the locomotives unavailable
            if(newGameState == GameState.DrawingTrainCards)
            {
                Game.Board.ChangeFaceUpLocomotiveStatus(isAvailable: false);
            }

            //if the player's turn has finished, change status of locomotives to available
            else if (newGameState == GameState.WaitingForPlayerMove)
            {
                Game.Board.ChangeFaceUpLocomotiveStatus(isAvailable: true);
            }

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerDrawTrainCard
            };
        }

        private GameState UpdateGameState(bool isTurnFinished = false)
        {
            //drawn a locomotive card, OR
            //already drawn one card, so after this one, the player's turn will end

            if (isTurnFinished || Game.GameState == GameState.DrawingTrainCards)
            {
                Game.UpdateStateNextPlayerTurn();

                return Game.GameState;
            }

            //the first card from this player's drawing cards turn
            else if (Game.GameState == GameState.WaitingForPlayerMove)
            {
                Game.GameState = GameState.DrawingTrainCards;
            }

            return Game.GameState;
        }
    }
}