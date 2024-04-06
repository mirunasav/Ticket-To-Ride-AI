using TicketToRide.Model.Enums;

namespace TicketToRide.Model.Cards
{
    public class TrainCard : Card
    {
        public TrainColor Color { get; set; }

        public bool IsAvailable { get; set; } = true;

        public override string ToString()
        {
            return $"Train card color : {Color}";
        }
    }
}
