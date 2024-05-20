using TicketToRide.Model.Enums;

namespace TicketToRide.Controllers.Responses
{
    public class CanClaimRouteResponse : MakeMoveResponse
    {
        public IList<TrainColor> TrainColorsWhichCanBeUsed { get; set; } = new List<TrainColor>();

        public IList<Model.GameBoard.Route> Route { get; set; }
    }
}
