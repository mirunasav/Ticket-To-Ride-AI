using TicketToRide.Model;
using TicketToRide.Model.Players;

namespace TicketToRide.Controllers.Responses
{
    public class RunGameResponse : MakeMoveResponse
    {
        public List<Player> Players { get; set; }

        public List<Player> Winners { get; set; }

        public int LongestContPathLength { get; set; }

        public int LongestContPathPlayerIndex { get; set; }

        public RunGameResponse(
            bool isValid,
            string message,
            List<Player> players, 
            List<Player> winners, 
            int longestContPathLength, 
            int longestContPathPlayerIndex)
        {
            IsValid = isValid;
            Message = message;
            Players = players;
            Winners = winners;
            LongestContPathLength = longestContPathLength;
            LongestContPathPlayerIndex = longestContPathPlayerIndex;
        }
    }
}
