using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class DrawDestinationCardMove : Move
    {
        public DrawDestinationCardMove(Game game, int playerIndex)
            : base(game, playerIndex) { }

        public override MakeMoveResponse Execute()
        {
            //draw 3 random destination cards
            //return them to player aka show on the screen? or add in destination card deck
            //instead of adding to hand, return them and after they are selected, remove from deck
            //and add to player hand

            //cheat: make move to try to post destination cards but others
            //mark the 3 destination cards as waitingToBeChosen.
            //check if the cards received are the ones waiting to be chosen
            //if yes, allow the move; at the end if any of the cards are returned to the deck,
            //unmark them

            var destinationCards = Game.Board.DestinationCards.Take(3).ToList();

            foreach(var card in destinationCards)
            {
                card.IsWaitingToBeChosen = true;
            }

            //Game.GetPlayer(playerIndex).PendingDestinationCards.AddRange(destinationCards);

            Game.GameState = Model.Enums.GameState.ChoosingDestinationCards;

            return new DrawDestinationCardsResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasDrawnDestinationCards,
                DrawnDestinationCards = destinationCards
            };
        }
    }
}
