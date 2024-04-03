using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Model.Players
{
    public class Player
    {
        public string Name { get; set; }

        public int Points { get; set; } = 0;

        public int RemainingTrains { get; set; } = 45;
        public PlayerColor Color { get; set; }

        public List<DestinationCard> PendingDestinationCards { get; set; } = new List<DestinationCard>();

        public List<DestinationCard> CompletedDestinationCards { get; set; } = new List<DestinationCard>();

        public List<TrainCard> Hand { get; set; } = new List<TrainCard>();

        //private Dictionary<PlayerActions, Action> ActionMap { get; set; }

        public Player(string name, PlayerColor color)
        {
            this.Name = name;
            this.Color = color;
        }

        //for now : draw 2 cards from the hidden deck
        public void Act(PlayerActions action, Board board)
        {
            switch (action)
            {
                case PlayerActions.DrawTrainCard:
                    DrawTrainCard(board);
                    break;
                default:
                    break;
            }
        }

        private void DrawTrainCard(Board board)
        {
            var cards = board.Deck.Pop(2);
            Hand.AddRange(cards);
        }

    }
}
