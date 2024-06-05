using TicketToRide.Model.Enums;

namespace TicketToRide.Model.Cards
{
    public class DestinationCard : Card
    {
        public City Origin { get; set; }

        public City Destination { get; set; }

        public int PointValue { get; set; }

        public bool IsWaitingToBeChosen { get; set; } = false;

        public DestinationCard() { }
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
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            DestinationCard other = (DestinationCard)obj;
            return Origin.Equals(other.Origin) &&
                   Destination.Equals(other.Destination) &&
                   PointValue == other.PointValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Origin.GetHashCode();
                hash = hash * 23 + Destination.GetHashCode();
                hash = hash * 23 + PointValue.GetHashCode();
                return hash;
            }
        }
    }

}
