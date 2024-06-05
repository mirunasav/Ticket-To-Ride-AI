using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using Route = TicketToRide.Model.GameBoard.Route;

namespace TicketToRide.Model.Players
{
    public class Player
    {
        public string Name { get; set; }

        public int Points { get; set; } = 0;

        public int RemainingTrains { get; set; } = 45;

        public PlayerColor Color { get; set; }

        public int PlayerIndex { get; set; } = 0;

        public List<DestinationCard> PendingDestinationCards { get; set; } = new List<DestinationCard>();

        public List<DestinationCard> CompletedDestinationCards { get; set; } = new List<DestinationCard>();

        public List<TrainCard> Hand { get; set; } = new List<TrainCard>();

        public int NumberOfTrainCards { get; set; }

        public int NumberOfPendingDestinationCards { get; set; }

        public int NumberOfCompletedDestinationCards { get; set;}

        public RouteGraph ClaimedRoutes { get; private set; } = new RouteGraph();

        public bool IsBot { get; set; } = false;

        //private Dictionary<PlayerActions, Action> ActionMap { get; set; }

        public Player()
        {

        }

        public Player(string name, PlayerColor color, int index)
        {
            this.Name = name;
            this.Color = color;
            ClaimedRoutes = new RouteGraph();
            PlayerIndex = index;
        }
       
        public int GetLocomotiveCount()
        {
           return Hand.Count(card => card.Color == TrainColor.Locomotive);
        }

        public int GetCardsOfColor(TrainColor color)
        {
            return Hand.Where(card => card.Color == color).Count();
        }

        public (bool hasCards, List<TrainCard> cardsToDiscard) RemoveCards(TrainColor color, int count)
        {
            var cardsToDiscard = new List<TrainCard>();
            var cardsOfColor = GetCardsOfColor(color);
            var locomotiveCount = GetLocomotiveCount();
            cardsOfColor += locomotiveCount;
           
            if(cardsOfColor < count)
            {
                return (false, cardsToDiscard);
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
                var card = Hand.ElementAt(indicesToRemove[i]);
                cardsToDiscard.Add(card);
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
                    var card = Hand.ElementAt(indicesToRemove[i]);
                    cardsToDiscard.Add(card);
                    Hand.RemoveAt(indicesToRemove[i]);
                }
            }

            return (true, cardsToDiscard);
        }

        public Player GetHiddenStatisticsPlayer()
        {
            var hiddentStatsPlayer = new Player(Name, Color, PlayerIndex)
            {
                Points = Points,
                RemainingTrains = RemainingTrains,
                NumberOfTrainCards = Hand.Count,
                NumberOfPendingDestinationCards = PendingDestinationCards.Count,
                NumberOfCompletedDestinationCards = CompletedDestinationCards.Count
            };

            return hiddentStatsPlayer;
        }

        public void AddCompletedRoute(Route route)
        {
            if (route.IsClaimed && route.ClaimedBy == Color)
            {
                ClaimedRoutes.AddRoute(route);
            }
        }

        public void MarkDestinationAsCompleted(DestinationCard destinationCard)
        {
            PendingDestinationCards.Remove(destinationCard);
            CompletedDestinationCards.Add(destinationCard);
        }

        public IList<DestinationCard> GetNewlyCompletedDestinations()
        {
            var newlyCompletedDestinations = new List<DestinationCard>();

            foreach (var card in PendingDestinationCards)
            {
                var isCompleted = ClaimedRoutes.AreConnected(card.Origin, card.Destination);

                if (isCompleted)
                {
                    newlyCompletedDestinations.Add(card);
                }
            }

            foreach(var destination in newlyCompletedDestinations)
            {
                MarkDestinationAsCompleted(destination);
            }

            return newlyCompletedDestinations;
        }

        public Dictionary<TrainColor, int> GroupedTrainColors()
        {
            Dictionary<TrainColor, int> colorCounts = [];

            foreach(var card in Hand)
            {
                if (colorCounts.ContainsKey(card.Color))
                {
                    colorCounts[card.Color] += 1;
                }
                else
                {
                    colorCounts[card.Color] = 1;
                }
            }

            return colorCounts;
        }

    }
}
