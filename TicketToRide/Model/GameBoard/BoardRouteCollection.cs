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

        public bool IsRouteValid(
            City origin,
            City destination,
            TrainColor trainColor = default,
            int length = default)
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

        public bool CanPlayerClaimRoute(Route route, Player player)
        {
            //aici? sau in alt serviciu
            //vad daca ruta e claimed
            if (route.IsClaimed)
            {
                //logica in plus pt jocuri cu 3-4 pers?
                return false;
            }

            // vad daca are suficiente trenuri
            if (player.RemainingTrains < route.Length)
            {
                return false;
            }

            //vad daca are cartonasele necesare
            var trainColorsWhichCanBeUsed = ColorsWithWhichRouteCanBeClaimed(route, player);

            return trainColorsWhichCanBeUsed.Count != 0;
        }
        public IList<TrainColor> ColorsWithWhichRouteCanBeClaimed(Route route, Player player)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            var cardsWhichCanBeUsed = new List<TrainColor>();
            var locomotiveCount = player.Hand.Count(card => card.Color == TrainColor.Locomotive);

            if (route.Color == TrainColor.Grey)
            {
                var groupedCardsByColor = player.Hand
                     .Where(card => card.Color != TrainColor.Locomotive)
                    .GroupBy(card => card.Color);

                var groupsOfValidColors = groupedCardsByColor
                    .Where(group => group.Count() + locomotiveCount >= route.Length).ToList();

                //return the colors of cards which can be used
                foreach (var group in groupsOfValidColors)
                {
                    cardsWhichCanBeUsed.Add(group.Key);
                }
            }
            else
            {
                var playerTrainsOfNecessaryColor = player.Hand
                     .Where(card => card.Color == route.Color).Count();

                playerTrainsOfNecessaryColor += locomotiveCount;

                if (playerTrainsOfNecessaryColor >= route.Length)
                {
                    cardsWhichCanBeUsed.Add(route.Color);
                }
            }

            return cardsWhichCanBeUsed;
        }

        public Route? GetRoute(
            City origin,
            City destination,
            TrainColor trainColor = default,
            int length = default,
            bool isClaimed = false)
        {
            var routeQuery = Routes.AsQueryable()
                .Where(r => (r.Origin == origin && r.Destination == destination)
                || (r.Origin == destination && r.Destination == origin));

            if (trainColor != default)
            {
                routeQuery = routeQuery.Where(r => r.Color == trainColor);
            }

            if (length != default)
            {
                routeQuery = routeQuery.Where(r => r.Length == length);
            }

            return routeQuery.FirstOrDefault();
        }
    }
}
