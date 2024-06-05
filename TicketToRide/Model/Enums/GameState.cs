namespace TicketToRide.Model.Enums
{
    public enum GameState
    {
        WaitingForPlayerMove,
        DrawingTrainCards,

        //when the player is trying to figure out which routes he can claim
        DecidingAction,
        ChoosingDestinationCards,
        Ended,
        DrawingFirstDestinationCards,
        ChoosingFirstDestinationCards
    }
}
