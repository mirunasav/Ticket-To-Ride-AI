namespace TicketToRide.Controllers.Responses
{
    public class MultipleGameResponse
    {
        public Dictionary<int, int> NumberOfGamesWonByEachPlayer { get; set; } = new Dictionary<int, int>();
        
        public List<GameSummaryResponse> GameSummaries { get; set; } = new List<GameSummaryResponse>();
    }
}
