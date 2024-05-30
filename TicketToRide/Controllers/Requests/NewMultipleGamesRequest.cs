namespace TicketToRide.Controllers.Requests
{
    public class NewMultipleGamesRequest : NewGameRequest
    {
        public int NumberOfGames { get; set; }
    }
}
