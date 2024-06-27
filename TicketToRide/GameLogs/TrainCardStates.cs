namespace TicketToRide.GameLogs
{
    public class TrainCardStates
    {
        public List<TrainCardsState> CardStates { get; set; } = new List<TrainCardsState>();

        public TrainCardStates() { }

        public TrainCardStates(List<TrainCardsState> cards) { CardStates = cards; }
    }
}
