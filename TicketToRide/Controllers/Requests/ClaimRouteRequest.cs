using TicketToRide.Model.Enums;

namespace TicketToRide.Controllers.Requests
{
    public class ClaimRouteRequest
    {
        public int PlayerIndex { get; set; }

        public string ColorUsed { get; set; }

        //sau direct route?
        public string OriginCity { get; set; }

        public string DestinationCity { get; set; }
    }
}
