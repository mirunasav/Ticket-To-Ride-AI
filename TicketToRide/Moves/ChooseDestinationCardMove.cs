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
            Game game,
            int playerIndex,
            List<DestinationCard> chosenDestinationCards,
            List<DestinationCard> notChosenDestinationCards)
            : base(game, playerIndex)
        {
            ChosenDestinationCards = chosenDestinationCards;
            NotChosenDestinationCards = notChosenDestinationCards;
        }

        public override MakeMoveResponse Execute()
        {
            //get the ones marked as waiting to be chosen 
            //update game state
            foreach(var chosenCard in ChosenDestinationCards)
            {
                Game.GetPlayer(PlayerIndex).PendingDestinationCards.Add(chosenCard);
                Game.Board.DestinationCards.Remove(chosenCard);
            }

            foreach (var notChosen in NotChosenDestinationCards)
            {
                notChosen.IsWaitingToBeChosen = false;
            }

            Game.UpdateStateNextPlayerTurn();

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasChosenDestinationCards
            };
        }
    }
}
