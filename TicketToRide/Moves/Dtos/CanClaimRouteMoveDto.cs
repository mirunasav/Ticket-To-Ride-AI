using TicketToRide.Model.Enums;

namespace TicketToRide.Moves.Dtos
{
    public class CanClaimRouteMoveDto : MoveDto
    {
        public City Origin { get; set; }

        public City Destination { get; set; }
        public IList<Model.GameBoard.Route> Route { get; set; }

        public CanClaimRouteMoveDto(IList<Model.GameBoard.Route> route, City origin, City destination)
        {
            Route = route;
            Origin = origin;
            Destination = destination;
        }

        public CanClaimRouteMoveDto(CanClaimRouteMove move)
        {
            Origin = move.Origin;
            Destination = move.Destination;
            Route = move.Route;
        }
    }
}
