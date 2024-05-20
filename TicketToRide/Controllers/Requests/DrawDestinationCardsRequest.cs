using TicketToRide.Model.Cards;

namespace TicketToRide.Controllers.Requests
{
    public class DrawDestinationCardsRequest
    {
        public int PlayerIndex { get; set; }

        public IList<ChosenDestinationCard> DestinationCards { get; set; }

    }
}
