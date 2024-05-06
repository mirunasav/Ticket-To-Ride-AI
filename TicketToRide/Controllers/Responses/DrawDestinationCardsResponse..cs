using TicketToRide.Model.Cards;

namespace TicketToRide.Controllers.Responses
{
    public class DrawDestinationCardsResponse : MakeMoveResponse
    {
        public IList<DestinationCard> DrawnDestinationCards { get; set; }
    }
}
