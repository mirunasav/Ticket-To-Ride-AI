using TicketToRide.Model.Enums;

namespace TicketToRide.Moves.Dtos
{
    public class ClaimRouteMoveDto : MoveDto
    {
        public IList<Model.GameBoard.Route> Route { get; set; }

        public TrainColor ColorUsed { get; set; }

        public City Origin { get; set; }

        public City Destination { get; set; }

        public ClaimRouteMoveDto(IList<Model.GameBoard.Route> route, TrainColor colorUsed, City origin, City destination)
        {
            Route = route;
            ColorUsed = colorUsed;
            Origin = origin;
            Destination = destination;
        }

        public ClaimRouteMoveDto(ClaimRouteMove move)
        {
            Route = move.Route;
            ColorUsed = move.ColorUsed;
            Origin = move.Origin;
            Destination = move.Destination;
        }
    }
}
