using TicketToRide.Model.Enums;

namespace TicketToRide.Controllers.Requests
{
    public class CanClaimRouteRequest
    {
        public int PlayerIndex { get; set; }

        public City Origin {  get; set; }

        public City Destination {  get; set; }
    }
}
