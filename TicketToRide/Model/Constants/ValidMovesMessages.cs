namespace TicketToRide.Model.Constants
{
    public class ValidMovesMessages
    {
        public const string ValidDrawTrainCardMove = "This player will now draw train cards";
        public const string PlayerDrawTrainCard = "The player has drawn a train card";
        public const string PlayerFinishDrawingTrainCards = "The player has drawn a train card and has finished his turn";

        public const string PlayerCanClaimRoute = "The player can claim this route";
        public const string PlayerHasClaimedRoute = "The player has claimed this route";

        public const string PlayerHasDrawnDestinationCards = "The player has drawn destination cards and has to keep at least one.";
        public const string PlayerHasChosenDestinationCards = "The player has chosen which destination cards to keep.";

        public const string GameHasEndedNoMovesLeft = "The game has because a player has no possible moves left.";
       
        public const string ValidMove = "Move is valid";

        public const string Ok = "OK";
    }
}
