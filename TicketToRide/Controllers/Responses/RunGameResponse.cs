using TicketToRide.Model;
using TicketToRide.Model.Players;

namespace TicketToRide.Controllers.Responses
{
    public class RunGameResponse : MakeMoveResponse
    {
        public string GameLogFile { get; set; }

        public string InitialGameStateFile { get; set; }

        public string TrainCardDeckStatesFileName { get; set; }

        public List<Player> Players { get; set; }

        public List<Player> Winners { get; set; }

        public int LongestContPathLength { get; set; }

        public int LongestContPathPlayerIndex { get; set; }
        public Dictionary<string, int> routesClaimed { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> numberOfRoutesClaimedForCity { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> numberOfDestinationCardsInWinningGames { get; set; } = new Dictionary<string, int>();

        public RunGameResponse(
            string gameLogFile,
            string initialGameStateFile,
            string trainCardDeckStatesFileName,
            Dictionary<string, int> routesClaimed,
            Dictionary<string, int> numberOfRoutesClaimedForCity,
            Dictionary<string, int> numberOfDestinationCardsInWinningGames,
            bool isValid,
            string message,
            List<Player> players, 
            List<Player> winners, 
            int longestContPathLength, 
            int longestContPathPlayerIndex)
        {
            InitialGameStateFile = initialGameStateFile;
            GameLogFile = gameLogFile;
            TrainCardDeckStatesFileName = trainCardDeckStatesFileName;
            IsValid = isValid;
            Message = message;
            Players = players;
            Winners = winners;
            LongestContPathLength = longestContPathLength;
            LongestContPathPlayerIndex = longestContPathPlayerIndex;
            this.routesClaimed = routesClaimed;
            this.numberOfRoutesClaimedForCity = numberOfRoutesClaimedForCity;
            this.numberOfDestinationCardsInWinningGames = numberOfDestinationCardsInWinningGames;
        }
    }
}
