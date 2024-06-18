namespace TicketToRide.Controllers
{
    public class GameStatistics
    {
        public Dictionary<int, int> NumberOfGamesWonByEachPlayer { get; set; } = new Dictionary<int, int>();

        public Dictionary <int, int> NumberOfLongestPathPrizes { get; set; } = new Dictionary<int, int>();
       
        public Dictionary <int, int> MaxPointsPerPlayer { get; set; } = new Dictionary<int, int>();

        public Dictionary <int, double> NumberOfFinishedDestinationTickets { get; set; } = new Dictionary<int, double>();

        public Dictionary<int, double> PlayerPointAverage { get; set; }  = new Dictionary<int, double>();

        public int BiggestPointDifference { get; set; } = 0;

        public int SmallestPointDifference { get; set; } = int.MaxValue;

        public Dictionary<string, int> routesClaimed { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> numberOfRoutesClaimedForCity { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> numberOfDestinationCardsInWinningGames { get; set; } = new Dictionary<string, int>();


        public void ComputePlayerPointAverage(int numberOfGames)
        {
            foreach (var key in PlayerPointAverage.Keys.ToList())
            {
                PlayerPointAverage[key] /= numberOfGames;
            }
        }

        public void ComputeAvgDestinationTickets(int numberOfGames)
        {
            foreach (var key in NumberOfFinishedDestinationTickets.Keys.ToList())
            {
                NumberOfFinishedDestinationTickets[key] /= numberOfGames;
            }
        }
    }
}
