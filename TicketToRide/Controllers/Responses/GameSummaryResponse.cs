using TicketToRide.Model.Players;

namespace TicketToRide.Controllers.Responses
{
    public class GameSummaryResponse
    {
        public string GameLogFileName {  get; set; }
        public string InitialGameStateFileName {  get; set; }
        public string TrainCardDeckStatesFileName {  get; set; }
       
        public List<int> WinnerPlayerIndex { get; set; } = new List<int>();
      
        public List<Player> Players { get; set; } = new List<Player>();

        public int LongestContPathLength { get; set; }

        public int LongestContPathPlayerIndex { get; set; }

    }
}
