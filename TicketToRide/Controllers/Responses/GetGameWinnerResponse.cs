using TicketToRide.Model.Players;

namespace TicketToRide.Controllers.Responses
{
    public class GetGameWinnerResponse : MakeMoveResponse
    {
        public IList<Player> Winners { get; set; }
    }
}
