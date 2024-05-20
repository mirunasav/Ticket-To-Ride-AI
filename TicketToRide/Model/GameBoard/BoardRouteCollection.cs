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

        public IList<Route> GetRoute(
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

            return routeQuery.ToList();
        }

        public Route? GetRoute(City origin, City destination, TrainColor trainColor)
        {
            return Routes.Where(r => (r.Color == trainColor || r.Color == TrainColor.Grey) &&
            ((r.Origin == origin && r.Destination == destination)
                || (r.Origin == destination && r.Destination == origin))).FirstOrDefault();
        }
    }
}
