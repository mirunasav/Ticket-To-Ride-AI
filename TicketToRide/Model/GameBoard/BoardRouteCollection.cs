using System.Drawing;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;

namespace TicketToRide.Model.GameBoard
{
    public class BoardRouteCollection
    {
        public IList<Route> Routes { get; set; } = new List<Route>();

        public void AddRoute(City origin, City destination, TrainColor trainColor, int length)
        {
            Routes.Add(new Route(origin, destination, trainColor, length));
        }

        public bool IsRouteValid(City origin, City destination, TrainColor trainColor, int length)
        {
            var route = GetRoute(origin, destination, trainColor, length);

            return route != null;
        }

        public Route ClaimRoute(City origin, City destination, TrainColor trainColor, int length, Player player)
        {
            var route = GetRoute(origin, destination, trainColor, length);

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            route.IsClaimed = true;
            route.ClaimedBy = player.Color;

            var points = route.PointValue;

            player.Points += points;
            player.RemainingTrains -= route.Length;

            //remove cards from player hand
            var cardsToRemove = player.Hand.Take(route.Length).Where(card => card.Color == route.Color).ToList();
            foreach (var card in cardsToRemove)
            {
                player.Hand.Remove(card);
            }
            return route;
        }

        public bool CanClaimRoute(City origin, City destination, TrainColor trainColor, int length, Player player)
        {
            var route = GetRoute(origin, destination, trainColor, length);

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            var playerTrainsOfNecessaryColor = 0;

            if (route.Color == TrainColor.Grey)
            {
                //aici tb sa fie toate de aceeasi culoare, nu de orice culoare si amestecate
                playerTrainsOfNecessaryColor = player.Hand.Count();
            }
            else
            {
                playerTrainsOfNecessaryColor = player.Hand
                    .Where(card => card.Color == route.Color || card.Color == TrainColor.Locomotive).Count();
            }

            return playerTrainsOfNecessaryColor >= route.Length;
        }

        private Route? GetRoute(City origin, City destination, TrainColor trainColor, int length, bool isClaimed = false)
        {
            var route = Routes.FirstOrDefault(r => r.Origin == origin
           && r.Destination == r.Destination
           && r.Color == trainColor
           && r.Length == length
           && r.IsClaimed == isClaimed);

            return route;
        }

    }
}
