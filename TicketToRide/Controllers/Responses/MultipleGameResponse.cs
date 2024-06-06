namespace TicketToRide.Controllers.Responses
{
    public class MultipleGameResponse
    {
        public GameStatistics GameStatistics { get; set; } = new GameStatistics();
        
        public List<GameSummaryResponse> GameSummaries { get; set; } = new List<GameSummaryResponse>();
    }
}
