using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class ChooseDestinationCardMove : Move
    {
        public List<DestinationCard> ChosenDestinationCards { get; set; }
        public List<DestinationCard> NotChosenDestinationCards { get; set; }

        public ChooseDestinationCardMove(
            int playerIndex,
            List<DestinationCard> chosenDestinationCards,
            List<DestinationCard> notChosenDestinationCards)
            : base(playerIndex)
        {
            ChosenDestinationCards = chosenDestinationCards;
            NotChosenDestinationCards = notChosenDestinationCards;
        }

        public override MakeMoveResponse Execute(Game game)
        {
            //get the ones marked as waiting to be chosen 
            //update game state
            foreach(var chosenCard in ChosenDestinationCards)
            {
                game.GetPlayer(PlayerIndex).PendingDestinationCards.Add(chosenCard);
                game.Board.DestinationCards.Remove(chosenCard);
            }

            foreach (var notChosen in NotChosenDestinationCards)
            {
                notChosen.IsWaitingToBeChosen = false;
            }

            game.UpdateStateNextPlayerTurn();

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasChosenDestinationCards
            };
        }
    }
}
