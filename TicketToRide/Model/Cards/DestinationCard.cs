using System.Drawing;
using TicketToRide.Model.Enums;

namespace TicketToRide.Model.Cards
{
    public class DestinationCard : Card
    {
        public City Origin { get; set; }

        public City Destination { get; set; }

        public int PointValue { get; set; }

        public bool IsWaitingToBeChosen { get; set; } = false;

        public DestinationCard(City origin, City destination, int points)
        {
            this.Origin = origin;
            this.Destination = destination;
            this.PointValue = points;
        }

        public override string ToString()
        {
            return $"Destination card from '{Origin}' to '{Destination}'; {PointValue}p";
        }
    }

}
