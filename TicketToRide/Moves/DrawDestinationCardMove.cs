using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class DrawDestinationCardMove : Move
    {
        public DrawDestinationCardMove(int playerIndex)
            : base(playerIndex) { }

        public override MakeMoveResponse Execute(Game game)
        {
            var destinationCards = game.Board.DestinationCards.Take(3).ToList();

            foreach (var card in destinationCards)
            {
                card.IsWaitingToBeChosen = true;
            }

            if (game.GameState == Model.Enums.GameState.DrawingFirstDestinationCards)
            {
                game.GameState = Model.Enums.GameState.ChoosingFirstDestinationCards;
            }
            else
            {
                game.GameState = Model.Enums.GameState.ChoosingDestinationCards;
            }

            return new DrawDestinationCardsResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasDrawnDestinationCards,
                DrawnDestinationCards = destinationCards
            };
        }

    }
}