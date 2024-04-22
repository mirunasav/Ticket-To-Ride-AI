using TicketToRide.Model.Enums;

namespace TicketToRide.Controllers.Requests
{
    public class ClaimRouteRequest
    {
        public int PlayerIndex { get; set; }

        public TrainColor ColorUsed { get; set; }

        //sau direct route?
        public Model.GameBoard.Route Route { get; set; }
    }
}
