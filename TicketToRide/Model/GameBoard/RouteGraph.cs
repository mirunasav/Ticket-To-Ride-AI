using Microsoft.AspNetCore.Components.Forms;
using QuickGraph.Graphviz.Dot;
using System.Drawing;
using TicketToRide.Helpers;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;

namespace TicketToRide.Model.GameBoard
{
    public class RouteGraph
    {
        public HashSet<City> Cities { get; set; }
        public List<Edge> Edges { get; set; }

        public RouteGraph()
        {
            Cities = new HashSet<City>();
            Edges = new List<Edge>();
        }

        public RouteGraph(List<Route> routes)
        {
            Cities = new HashSet<City>();
            Edges = new List<Edge>();
            foreach (var route in routes)
            {
                AddRoute(route);
            }
        }

        public void AddCity(City city)
        {
            Cities.Add(city);
        }

        public void AddRoute(Route route)
        {
            var equivalentEdge = GetEquivalentEdge(route);

            if (equivalentEdge != null)
            {
                equivalentEdge.Routes.Add(route);
            }
            else
            {
                AddCity(route.Origin);
                AddCity(route.Destination);
                Edges.Add(new Edge(new List<Route> { route }));
            }
        }

        public void RemoveRoute(Route route)
        {
            var equivalentEdge = GetEquivalentEdge(route);

            if (equivalentEdge != null)
            {
                equivalentEdge.Routes.Remove(route);

                if (equivalentEdge.Routes.Count == 0)
                {
                    Edges.Remove(equivalentEdge);
                }

                //remove cities if they no longer have any routes
                if (!HasRoutes(route.Origin))
                {
                    RemoveCity(route.Origin);
                }

                if (!HasRoutes(route.Destination))
                {
                    RemoveCity(route.Destination);
                }
            }
        }

        public Edge? GetEquivalentEdge(Route route)
        {
            return Edges
                 .FirstOrDefault(e => e.Routes.Where(r => r.Origin == route.Origin && r.Destination == route.Destination).Any());
        }

        public List<Edge> GetEdges()
        {
            return Edges;
        }

        public bool AreConnected(City city1, City city2)
        {
            HashSet<City> visited = new HashSet<City>();
            Queue<City> queue = new Queue<City>();

            queue.Enqueue(city1);
            visited.Add(city1);

            while (queue.Any())
            {
                City currentCity = queue.Dequeue();

                if (currentCity == city2)
                {
                    return true;
                }

                foreach (var edge in Edges.Where(e => e.Origin == currentCity || e.Destination == currentCity))
                {
                    var nextCity = edge.Origin == currentCity ? edge.Destination : edge.Origin;
                    if (!visited.Contains(nextCity))
                    {
                        queue.Enqueue(nextCity);
                        visited.Add(nextCity);
                    }
                }
            }

            return false;
        }

        public bool ContainsRoute(Route route)
        {
            return Edges.Where(e => e.Routes.Contains(route)).Any();
        }

        public (int, List<Edge>) LongestContinuousPath()
        {
            int longestPathLength = 0;
            List<Edge> longestPathEdges = new List<Edge>();

            foreach (var city in Cities)
            {
                var visitedEdges = new HashSet<Edge>();
                var visitedCities = new HashSet<City>();
                var currentPathEdges = new List<Edge>();
                var (pathLength, pathEdges) = DFS(city, visitedEdges, 0, currentPathEdges);

                if (pathLength > longestPathLength)
                {
                    longestPathLength = pathLength;
                    longestPathEdges = pathEdges;
                }
            }

            return (longestPathLength, longestPathEdges);
        }

        private (int, List<Edge>) DFS(City currentCity, HashSet<Edge> visitedEdges, int currentLength, List<Edge> currentPathEdges)
        {
            int maxLength = currentLength;
            List<Edge> maxPathEdges = new List<Edge>(currentPathEdges);

            foreach (var edge in Edges.Where(e => (e.Origin == currentCity || e.Destination == currentCity) && !visitedEdges.Contains(e)))
            {
                visitedEdges.Add(edge);
                currentPathEdges.Add(edge);
                var nextCity = edge.Origin == currentCity ? edge.Destination : edge.Origin;
                var (pathLength, pathEdges) = DFS(nextCity, visitedEdges, currentLength + edge.Cost, currentPathEdges);

                if (pathLength > maxLength)
                {
                    maxLength = pathLength;
                    maxPathEdges = new List<Edge>(pathEdges);
                }

                currentPathEdges.Remove(edge);
                visitedEdges.Remove(edge);
            }

            return (maxLength, maxPathEdges);
        }

        private void RemoveCity(City city)
        {
            Cities.Remove(city);
        }

        private bool HasRoutes(City city)
        {
            return Edges.Any(edge => edge.Routes.Any(route => route.Origin == city || route.Destination == city));
        }

        public List<Route> FindAllShortestPathsBetweenDestinationCards(
            List<DestinationCard> destinationCards,
            PlayerColor playerColor,
            int numberOfPlayers)
        {
            var routesInShortestPaths = new List<Route>();

            foreach (var card in destinationCards)
            {
                (int shortestDistance, List<Edge> shortestPath) = FindShortestPath(card.Origin, card.Destination, playerColor, numberOfPlayers);

                if (shortestDistance != int.MaxValue)
                {
                    //the destination is reachable
                    routesInShortestPaths.AddRange(shortestPath.SelectMany(e => e.Routes));
                }
            }

            return routesInShortestPaths.Distinct().ToList();
        }


        public List<Route> GetRemainingRoutesToConnectAllCities(List<DestinationCard> destinationCards, PlayerColor playerColor, BoardRouteCollection boardRouteCollection, int numberOfPlayers)
        {
            var shortestPathConnectingAllCities = GetShortestPathConnectingAllCities(destinationCards, playerColor, boardRouteCollection, numberOfPlayers);

            List<int> indicesToRemove = [];

            for (int i = 0; i < shortestPathConnectingAllCities.Count(); i++)
            {
                if (shortestPathConnectingAllCities[i].IsClaimed
                    && shortestPathConnectingAllCities[i].ClaimedBy == playerColor)
                {
                    indicesToRemove.Add(i);
                }
            }
            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                shortestPathConnectingAllCities.RemoveAt(indicesToRemove[i]);
            }

            return shortestPathConnectingAllCities;
        }

        public List<Route> GetShortestPathConnectingAllCities(List<DestinationCard> destinationCards, PlayerColor playerColor, BoardRouteCollection boardRouteCollection, int numberOfPlayers)
        {
            if (destinationCards.Count == 0)
            {
                return [];
            }

            //get the main paths between the cities on the destination cards
            var mainPaths = new List<Edge>();
            {
                for (int i = 0; i < destinationCards.Count; i++)
                {
                    var (distance, path) = FindShortestPath(destinationCards[i].Origin, destinationCards[i].Destination, playerColor, numberOfPlayers);

                    if (distance != int.MaxValue)
                    {
                        mainPaths.AddRange(path);
                    }
                }
            }

            // Step 1: Compute interconnecting paths
            var interconnectingPaths = new List<Edge>();
            for (int i = 0; i < destinationCards.Count - 1; i++)
            {
                for (int j = i + 1; j < destinationCards.Count; j++)
                {
                    var (distanceAC, pathAC) = FindShortestPath(destinationCards[i].Origin, destinationCards[j].Origin, playerColor, numberOfPlayers);
                    var (distanceAD, pathAD) = FindShortestPath(destinationCards[i].Origin, destinationCards[j].Destination, playerColor, numberOfPlayers);
                    var (distanceBC, pathBC) = FindShortestPath(destinationCards[i].Destination, destinationCards[j].Origin, playerColor, numberOfPlayers);
                    var (distanceBD, pathBD) = FindShortestPath(destinationCards[i].Destination, destinationCards[j].Destination, playerColor, numberOfPlayers);

                    var shortestPath = new List<Edge>();
                    if (distanceAC < distanceAD && distanceAC < distanceBC && distanceAC < distanceBD)
                    {
                        shortestPath = pathAC;
                    }
                    else if (distanceAD < distanceAC && distanceAD < distanceBC && distanceAD < distanceBD)
                    {
                        shortestPath = pathAD;
                    }
                    else if (distanceBC < distanceAC && distanceBC < distanceAD && distanceBC < distanceBD)
                    {
                        shortestPath = pathBC;
                    }
                    else
                    {
                        shortestPath = pathBD;
                    }

                    interconnectingPaths.AddRange(shortestPath);
                }
            }

            // Step 2: Merge paths
            var mergedPaths = mainPaths.Concat(interconnectingPaths).ToList();

            // Step 3: Return unique routes
            var uniqueRoutes = new HashSet<Route>(mergedPaths.SelectMany(p => p.Routes));
            return uniqueRoutes.ToList();
        }

        public (int, List<Edge>) FindShortestPath(City startCity, City endCity, PlayerColor playerColor, int numberOfPlayers)
        {
            var distances = Cities.ToDictionary(city => city, city => int.MaxValue);
            var previousEdges = new Dictionary<City, Edge>();
            var priorityQueue = new PriorityQueue<(City city, int distance), int>();

            distances[startCity] = 0;
            priorityQueue.Enqueue((startCity, 0), 0);

            while (priorityQueue.Count > 0)
            {
                var (currentCity, currentDistance) = priorityQueue.Dequeue();

                if (currentCity == endCity)
                {
                    break;
                }

                foreach (var edge in Edges.Where(e =>
                (e.Origin == currentCity || e.Destination == currentCity)
                && (e.CanPlayerUseEdge(playerColor) || e.CanBeClaimed(numberOfPlayers))))
                {
                    var relevantEdge = edge;
                    if (edge.Routes.Count > 1)
                    {
                        relevantEdge = edge.GetRelevantEdge(edge, playerColor);
                    }

                    var neighborCity = edge.Origin == currentCity ? edge.Destination : edge.Origin;
                    int distance = currentDistance;

                    //only add the cost for paths that are not claimed
                    if (!edge.Routes.Any(r => r.CanPlayerUseRoute(playerColor)))
                    {
                        distance += edge.Cost;
                    }

                    if (distance < distances[neighborCity])
                    {
                        distances[neighborCity] = distance;
                        previousEdges[neighborCity] = relevantEdge;
                        priorityQueue.Enqueue((neighborCity, distance), distance);
                    }
                }
            }

            if (distances[endCity] == int.MaxValue)
            {
                return (int.MaxValue, new List<Edge>());
            }

            List<Edge> path = new List<Edge>();
            City? current = endCity;

            while (current != null && current != startCity)
            {
                if (previousEdges.TryGetValue((City)current, out Edge edge))
                {
                    path.Add(edge);
                    current = edge.Origin == current ? edge.Destination : edge.Origin;
                }
                else
                {
                    break;
                }
            }

            path.Reverse();
            return (distances[endCity], path);
        }

        public Dictionary<TrainColor, int> GetLeastTrainColorsNeededForShorthestPaths(List<Route> shortestPathRoutes)
        {
            var trainCardsDictionary = new Dictionary<TrainColor, int>();

            foreach (var route in shortestPathRoutes)
            {

                if (trainCardsDictionary.ContainsKey(route.Color))
                {
                    if (route.Length < trainCardsDictionary[route.Color])
                    {
                        trainCardsDictionary[route.Color] = route.Length;
                    }
                }
                else
                {
                    trainCardsDictionary[route.Color] = route.Length;
                }
            }

            return trainCardsDictionary;
        }

        //for evaluation player when checking the availability of a path
        public bool IsPathAvailableToPlayer(List<GameBoard.Route> path, PlayerColor playerColor)
        {
            foreach (var route in path)
            {
                //if there is a route that is claimed and unusable
                if (route.IsClaimed && !route.CanPlayerUseRoute(playerColor))
                {
                    return false;
                }
            }
            return true;
        }

        //for evaluation player: find all paths between 2 cities, ordered by length
        public List<List<Route>> FindAllPaths(City start, City end, bool isFirstTurn, PlayerColor playerColor, int maxPaths, int numberOfPlayers)
        {
            List<List<Route>> resultPaths = new List<List<Route>>();
            Queue<(List<Route> Path, City CurrentCity)> pathsQueue = new Queue<(List<Route>, City)>();
            pathsQueue.Enqueue((new List<Route>(), start));
            int iterations = 0;

            while (pathsQueue.Count > 0 &&
                ((iterations < 10000 && resultPaths.Count < maxPaths)
                    || (resultPaths.Count == 0 && isFirstTurn)))
            {
                iterations++;
                var (currentPath, currentCity) = pathsQueue.Dequeue();

                // Check if we've reached the destination
                if (currentCity.Equals(end))
                {
                    resultPaths.Add(new List<Route>(currentPath));
                    continue; // Continue searching for other paths
                }

                foreach (var edge in GetEdgesFromCity(currentCity))
                {
                    foreach (var route in edge.Routes)
                    {
                        // Skip claimed routes if the player can't use them
                        if (route.IsClaimed && !route.CanPlayerUseRoute(playerColor))
                        {
                            continue;
                        }

                        //check if can be claimed
                        if (edge.CanBeClaimed(numberOfPlayers))
                        {
                            City nextCity = route.Origin.Equals(currentCity) ? route.Destination : route.Origin;

                            // Avoid cycles: Check if nextCity is already in the path
                            if (!IsCityInPath(currentPath, nextCity))
                            {
                                var newPath = new List<Route>(currentPath) { route };
                                pathsQueue.Enqueue((newPath, nextCity));
                            }
                        }

                    }
                }
            }

            return resultPaths.OrderBy(p => p.Sum(r => r.Length)).ToList();
        }
        private bool IsCityInPath(List<Route> path, City city)
        {
            return path.Any(r => r.Origin.Equals(city) || r.Destination.Equals(city));
        }

        private IEnumerable<Edge> GetEdgesFromCity(City city)
        {
            return Edges.Where(e => e.Origin.Equals(city) || e.Destination.Equals(city));
        }
    }

    public class Edge
    {
        public City Origin { get; set; }
        public City Destination { get; set; }

        public List<Route> Routes { get; set; }

        public int Cost { get; set; }

        public Edge()
        {

        }

        public Edge(List<Route> route)
        {
            Origin = route.ElementAt(0).Origin;
            Destination = route.ElementAt(0).Destination;
            Cost = route.ElementAt(0).Length;
            Routes = route;
        }

        public bool CanBeClaimed(int numberOfPlayers)
        {
            var routes = new List<Route>();
            foreach (var route in Routes)
            {
                routes.Add(new Route(route));
            }

            var isClaimed = IsRouteClaimed(routes, numberOfPlayers);
            return !isClaimed;
        }

        public bool CanPlayerUseEdge(PlayerColor color)
        {
            return Routes.Any(r => r.CanPlayerUseRoute(color));
        }

        //given an edge with a double route, return the edge
        //with only the relevant route, the claimed one or the one that can be claimed
        public Edge GetRelevantEdge(Edge edge, PlayerColor playerColor)
        {
            var routes = new List<Route>();
            foreach (var route in edge.Routes)
            {
                if (route.CanPlayerUseRoute(playerColor))
                {
                    routes.Add(route);
                    break;
                }
            }

            if (routes.Count == 1)//a usable route exists
            {
                return new Edge(routes);
            }
            //otherwise, there is no route that can be used, so get the first one that can be claimed
            foreach (var route in edge.Routes)
            {
                if (route.IsClaimed == false)
                {
                    routes.Add(route);
                    break;
                }
            }

            return new Edge(routes);
        }

        private bool IsRouteClaimed(IList<Model.GameBoard.Route> routeCollection, int numberOfPlayers)
        {
            //if the route is not double, just check if the first element of the collection 
            //is claimed
            if (routeCollection.Count == 1)
            {
                return routeCollection.ElementAt(0).IsClaimed;
            }

            //else, check if any of the routes is claimed and the game has more than 3 players

            var claimedRoutes = routeCollection.Where(r => r.IsClaimed).ToList();

            foreach (var claimedRoute in claimedRoutes)
            {
                routeCollection.Remove(claimedRoute);
            }

            if (claimedRoutes.Count == 1 && numberOfPlayers < GameConstants.MinNumberOfPlayersForWhichDoubleRoutesCanBeUsed)
            {
                return true;
            }

            if (claimedRoutes.Count == 2 && numberOfPlayers >= GameConstants.MinNumberOfPlayersForWhichDoubleRoutesCanBeUsed)
            {
                return true;
            }

            return false;
        }
    }

}

