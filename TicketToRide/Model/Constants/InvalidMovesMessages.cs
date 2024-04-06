using System.ComponentModel;

namespace TicketToRide.Model.Constants
{
    public class InvalidMovesMessages
    {
        public const string NotThisPlayersTurn = "It is not this player's turn to move";
        public const string InvalidPlayerIndex = "The player index is invalid";
        public const string UninitializedGame = "The game has not been initialized";
        public const string InvalidActionForCurrentGameState = "This action cannot be performed in this current game state";
        public const string NotEnoughTrainCardsToDrawFrom = "There are no train cards left to draw from";
        public const string InvalidFaceUpCardIndex = "You cannot draw this face up card";
    }
}
