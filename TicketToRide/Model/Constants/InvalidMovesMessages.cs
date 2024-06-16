
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

        public const string RouteDoesNotExist = "The route with this origin and this destination does not exist";
        public const string RouteAlreadyClaimed = "The route with has already been claimed";
        public const string NotEnoughTrainPieces = "The player does not have enough train pieces left";
        public const string PlayerDoesNotHaveResources = "The player does not have the necessary train cards to claim this route";
        public const string InvalidColorForRoute = "This train color cannot be used to claim this route";

        public const string DestinationDoesNotExist = "The destination with this origin and this destination does not exist";
        public const string DestinationIsNotAnOption = "This destination was not an option";
        public const string InvalidCityNumber = "The destination or origin cities are invalid";
        public const string PlayerAlreadyClaimedRoute = "The player has already claimed this route";

        public const string NotEnoughDestinationCards = "There are not enough destination cards left";

        public const string GameNotEndedYet = "Cannot compute game outcome. Game not ended yet";

        public const string PlayerNotBot = "This player is not a bot";

        public const string NotAllPlayersAreBots = "Game cannot be run because not all players are bots";
    }
}
