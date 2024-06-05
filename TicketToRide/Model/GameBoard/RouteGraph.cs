using TicketToRide.Model.Cards;
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
            AddCity(route.Origin);
            AddCity(route.Destination);
            Edges.Add(new Edge(route));
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
            return Edges.Where(e => e.Route == route).Any();
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

        public void RemoveEdge(Route route)
        {
            // Find the edge to remove
            var edgeToRemove = Edges.FirstOrDefault(e => (e.Route == route));

            if (edgeToRemove != null)
            {
                // Remove the edge
                Edges.Remove(edgeToRemove);
            }
        }

        public void RemoveEdges(List<Route> routes)
        {
            foreach (var route in routes)
            {
                RemoveEdge(route);
            }
        }

        public List<Route> FindAllShortestPathsBetweenDestinationCards(
            List<DestinationCard> destinationCards, 
            PlayerColor playerColor, 
            BoardRouteCollection boardRouteCollection)
        {
            var routesInShortestPaths = new List<Route>();

            foreach (var card in destinationCards)
            {
                (int shortestDistance, List<Edge> shortestPath) = FindShortestPath(card.Origin, card.Destination, playerColor, boardRouteCollection);

                if (shortestDistance != int.MaxValue)
                {
                    //the destination is reachable
                    routesInShortestPaths.AddRange(shortestPath.Select(e => e.Route));
                }
            }

            return routesInShortestPaths.Distinct().ToList();
        }

        public (int, List<Edge>) FindShortestPath(City startCity, City endCity, PlayerColor playerColor, BoardRouteCollection boardRouteCollection)
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
                && (!e.Route.IsClaimed || e.Route.CanPlayerUseRoute(playerColor))))
                {
                    if (edge.Route.IsClaimed || !boardRouteCollection.DoesEquivalentUsableRouteExist(edge.Route, playerColor))
                    {
                        //if the route is claimed or unique, no other usable equivalent usable routes exists
                        //meaning that the route is unique or it's double but claimed by other player

                        var neighborCity = edge.Origin == currentCity ? edge.Destination : edge.Origin;
                        int distance = currentDistance + edge.Cost;

                        if (distance < distances[neighborCity])
                        {
                            distances[neighborCity] = distance;
                            previousEdges[neighborCity] = edge;
                            priorityQueue.Enqueue((neighborCity, distance), distance);
                        }
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

            //remove the paths which are already claimed?
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
    }

    public class Edge
    {
        public City Origin { get; set; }
        public City Destination { get; set; }
        public Route Route { get; set; }

        public int Cost { get; set; }

        public Edge(Route route)
        {
            Origin = route.Origin;
            Destination = route.Destination;
            Cost = route.Length;
            Route = route;
        }
    }

}

