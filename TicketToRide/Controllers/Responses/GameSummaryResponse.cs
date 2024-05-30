using TicketToRide.Model.Players;

namespace TicketToRide.Controllers.Responses
{
    public class GameSummaryResponse
    {
        public List<int> WinnerPlayerIndex { get; set; } = new List<int>();
        public List<Player> Players { get; set; } = new List<Player>();
    }
}
