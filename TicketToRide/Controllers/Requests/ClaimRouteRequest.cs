using TicketToRide.Model.Enums;

namespace TicketToRide.Controllers.Requests
{
    public class ClaimRouteRequest
    {
        public int PlayerIndex { get; set; }
        public City Origin {  get; set; }

        public City Destination {  get; set; }

        public TrainColor TrainColor { get; set; }

        public int Length { get; set; }

    }
}
