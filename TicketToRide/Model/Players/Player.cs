using Microsoft.AspNetCore.Routing;
using System.Numerics;
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

        public int GetLocomotiveCount()
        {
           return Hand.Count(card => card.Color == TrainColor.Locomotive);
        }

        public int GetCardsOfColor(TrainColor color)
        {
            return Hand.Where(card => card.Color == color).Count();
        }

        public bool RemoveCards(TrainColor color, int count)
        {
            var cardsOfColor = GetCardsOfColor(color);
            var locomotiveCount = GetLocomotiveCount();
            cardsOfColor += locomotiveCount;
           
            if(cardsOfColor < count)
            {
                return false;
            }

            var removedCardCount  = 0;

            List<int> indicesToRemove = new List<int>();

            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Color == color)
                {
                    indicesToRemove.Add(i);
                    removedCardCount++;

                    if (indicesToRemove.Count == count)
                    {
                        break;
                    }
                }
            }

            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                Hand.RemoveAt(indicesToRemove[i]);
            }

            if(removedCardCount != count)
            {
                //we also need to remove locomotives
                var remainingCardsToRemove = count - removedCardCount;

                indicesToRemove.Clear();

                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand[i].Color == TrainColor.Locomotive)
                    {
                        indicesToRemove.Add(i);
                        removedCardCount++;

                        if (indicesToRemove.Count == remainingCardsToRemove)
                        {
                            break;
                        }
                    }
                }

                for (int i = indicesToRemove.Count - 1; i >= 0; i--)
                {
                    Hand.RemoveAt(indicesToRemove[i]);
                }
            }

            return true;
        }
        private void DrawTrainCard(Board board)
        {
            var cards = board.Deck.Pop(2);
            Hand.AddRange(cards);
        }

    }
}
