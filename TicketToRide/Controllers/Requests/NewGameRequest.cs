namespace TicketToRide.Controllers.Requests
{
    public class NewGameRequest
    {
        public int NumberOfPlayers { get; set; }
        
        public List<string> PlayerTypes { get; set; }
    }
}
