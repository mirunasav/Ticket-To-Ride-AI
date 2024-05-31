using TicketToRide.Model.Enums;

namespace TicketToRide.Model.GameBoard
{
    public class RouteOriginDestination
    {
        public City Origin { get; set; }

        public City Destination { get; set; }

        public RouteOriginDestination(City origin, City destination) 
        { 
            Origin = origin;
            Destination = destination;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RouteOriginDestination);
        }

        public bool Equals(RouteOriginDestination other)
        {
            if (other == null)
                return false;

            return (Origin == other.Origin && Destination == other.Destination) ||
                   (Origin == other.Destination && Destination == other.Origin);
        }

        public override int GetHashCode()
        {
            // Ensure that the hash code is order-independent
            int hash1 = Origin.GetHashCode();
            int hash2 = Destination.GetHashCode();

            return hash1 ^ hash2;
        }

        public static bool operator ==(RouteOriginDestination left, RouteOriginDestination right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(RouteOriginDestination left, RouteOriginDestination right)
        {
            return !(left == right);
        }
    }
}
