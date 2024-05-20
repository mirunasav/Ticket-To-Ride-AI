using TicketToRide.Model.Cards;

namespace TicketToRide.Controllers.Responses
{
    public class ClaimRouteResponse : MakeMoveResponse
    {
        public IList<TrainCard> PlayerRemainingCards { get; set; }

        public IList<DestinationCard> NewlyCompletedDestinations { get; set; }
    }
}
