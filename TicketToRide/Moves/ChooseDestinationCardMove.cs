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
        private bool isFromLog { get; set; } //find the object from the game which denotes the same card

        public ChooseDestinationCardMove(
            int playerIndex,
            List<DestinationCard> chosenDestinationCards,
            List<DestinationCard> notChosenDestinationCards,
            bool isFromLog = false)
            : base(playerIndex)
        {
            ChosenDestinationCards = chosenDestinationCards;
            NotChosenDestinationCards = notChosenDestinationCards;
            this.isFromLog = isFromLog;
        }

        public override MakeMoveResponse Execute(Game game)
        {
            
            //get the ones marked as waiting to be chosen 
            //update game state
            foreach (var chosenCard in ChosenDestinationCards)
            {
                var card = chosenCard;

                if (isFromLog)
                {
                    card = GetCardFromGame(game, chosenCard);

                }

                game.GetPlayer(PlayerIndex).PendingDestinationCards.Add(card);
                game.Board.DestinationCards.Remove(card);
            }

            foreach (var notChosen in NotChosenDestinationCards)
            {
                var card = notChosen;

                if (isFromLog)
                {
                    card = GetCardFromGame(game, notChosen);

                }

                card.IsWaitingToBeChosen = false;
            }

            if (game.GameState == Model.Enums.GameState.ChoosingFirstDestinationCards)
            {
                game.TryBeginGame();
            }
            else
            {
                game.UpdateStateNextPlayerTurn();
            }

            game.GetPlayer(PlayerIndex).GetNewlyCompletedDestinations();

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerHasChosenDestinationCards
            };
        }

        private DestinationCard GetCardFromGame(Game game, DestinationCard card)
        {
            var realCard = game.Board.DestinationCards.Where(c => c.Equals(card)).First();
            return realCard;
        }
    }
}
