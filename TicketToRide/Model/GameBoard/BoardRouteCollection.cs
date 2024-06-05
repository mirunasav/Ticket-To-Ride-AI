using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;

namespace TicketToRide.Model.GameBoard
{
    public class BoardRouteCollection
    {
        public List<Route> Routes { get; set; } = [];

        private Dictionary<RouteOriginDestination, List<Route>> RouteDictionary { get; set; } = new Dictionary<RouteOriginDestination, List<Route>>();

        public void AddRoute(City origin, City destination, TrainColor trainColor, int length)
        {
            var key = new RouteOriginDestination(origin, destination);
            var newRoute = new Route(origin, destination, trainColor, length);

            if (!RouteDictionary.TryGetValue(key, out List<Route>? value))
            {
                value = [];
                RouteDictionary[key] = value;
            }

            value.Add(newRoute);

            Routes.Add(newRoute);
        }

        public void AddRoute(Route route)
        {
            var key = new RouteOriginDestination(route.Origin, route.Destination);

            if (!RouteDictionary.TryGetValue(key, out List<Route>? value))
            {
                value = [];
                RouteDictionary[key] = value;
            }

            value.Add(route);
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

        public List<Route> GetRoute(
            City origin,
            City destination,
            TrainColor trainColor = default,
            int length = default,
            bool isClaimed = false)
        {

            var key1 = new RouteOriginDestination(origin, destination);

            var routeList = new List<Route>();

            if (RouteDictionary.TryGetValue(key1, out List<Route>? value1))
            {
                routeList.AddRange(value1);
            }

            var routeQuery = routeList.AsQueryable();

            if (trainColor != default)
            {
                routeQuery = routeQuery.Where(r => r.Color == trainColor);
            }

            if (length > 0)
            {
                routeQuery = routeQuery.Where(r => r.Length == length);
            }

            if (isClaimed)
            {
                routeQuery = routeQuery.Where(r => r.IsClaimed == isClaimed);
            }

            return routeQuery.ToList();
        }

        public Route? GetRoute(City origin, City destination, TrainColor trainColor, bool unclaimed = false)
        {
            var key1 = new RouteOriginDestination(origin, destination);

            List<Route> routes = new List<Route>();

            if (RouteDictionary.ContainsKey(key1))
            {
                routes.AddRange(RouteDictionary[key1]);
            }

            var routesQuery = routes.Where(r => (r.Color == trainColor || r.Color == TrainColor.Grey) &&
               ((r.Origin == origin && r.Destination == destination) || (r.Origin == destination && r.Destination == origin)));
            if (unclaimed)
            {
                routesQuery = routesQuery.Where(r => r.IsClaimed == false);
            }

            return routesQuery.FirstOrDefault();
        }

        public Route? GetRoute(Route route)
        {
            var key1 = new RouteOriginDestination(route.Origin, route.Destination);

            var routeList = new List<Route>();

            if (RouteDictionary.TryGetValue(key1, out List<Route>? value1))
            {
                routeList.AddRange(value1);
            }

            return routeList.FirstOrDefault(r => r.Equals(route));
        }

        public bool DoesEquivalentUsableRouteExist(Route route, PlayerColor playerColor)
        {
            var routes = GetRoute(route.Origin, route.Destination);
            if(routes.Any(r => r.IsClaimed && r.ClaimedBy == playerColor))
            {
                return true;
            }
            return false;
        }
    }
}