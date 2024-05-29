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

            foreach(var card in destinationCards)
            {
                card.IsWaitingToBeChosen = true;
            }

            //Game.GetPlayer(playerIndex).PendingDestinationCards.AddRange(destinationCards);

            game.GameState = Model.Enums.GameState.ChoosingDestinationCards;

            var gameLogMessage = CreateGameLogMessage(game.Players.ElementAt(PlayerIndex).Name);
            LogMove(game.GameLog, gameLogMessage);

            return new DrawDestinationCardsResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasDrawnDestinationCards,
                DrawnDestinationCards = destinationCards
            };
        }

        private string CreateGameLogMessage(string playerName)
        {
            return $"{playerName} has drawn destination cards.";
        }
    }
}
