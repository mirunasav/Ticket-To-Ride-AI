using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class ChooseDestinationCardMove : Move
    {
        public IList<DestinationCard> DestinationCards { get; set; }

        public ChooseDestinationCardMove(Game game, int playerIndex, IList<DestinationCard> destinationCards)
            : base(game, playerIndex)
        {
            DestinationCards = destinationCards;
        }
        public override MakeMoveResponse Execute()
        {
            //update game state
            Game.UpdateStateNextPlayerTurn();

            return new MakeMoveResponse
            {

            };
            //return the refused card : check if the card exists in the player's hand
            //put them in a list: waiting to be reviwed dest cards? or mark them in some way with a flag?

        }
    }
}
