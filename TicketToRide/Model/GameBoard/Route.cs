using TicketToRide.Model.Enums;

namespace TicketToRide.Model.GameBoard
{
    public class Route
    {
        public City Origin { get; set; }

        public City Destination { get; set; }

        public TrainColor Color { get; set; }

        public int Length { get; set; }

        public bool IsClaimed { get; set; }

        public PlayerColor? ClaimedBy { get; set; }

        public int PointValue
        {
            get
            {
                switch (Length)
                {
                    case 1: return 1;
                    case 2: return 2;
                    case 3: return 4;
                    case 4: return 7;
                    case 5: return 10;
                    case 6: return 15;
                    default: return 1;
                }

            }
            set { value = Length; }
        }

        public Route()
        {

        }

        public Route(City Origin, City Destination, TrainColor TrainColor, int Length)
        {
            this.Origin = Origin;
            this.Destination = Destination;
            this.Color = TrainColor;
            this.Length = Length;
        }

        public bool CanPlayerUseRoute(PlayerColor playerColor)
        {
            if (IsClaimed && ClaimedBy == playerColor)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Route other = (Route)obj;
            return Origin == other.Origin && Destination == other.Destination && Color == other.Color && Length == other.Length;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Origin, Destination, Color, Length);
        }
    }

}
