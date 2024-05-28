using TicketToRide.Model.Players;

namespace TicketToRide.Controllers.Responses
{
    public class GetGameWinnerResponse : MakeMoveResponse
    {
        public IList<Player> Winners { get; set; }

        public IList<Player> Players { get; set; }

        public int LongestContPathLength { get; set; }

        public int LongestContPathPlayerIndex { get; set; }
    }
}
