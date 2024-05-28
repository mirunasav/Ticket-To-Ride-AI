﻿using TicketToRide.Model.Enums;

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

        public void AddCity(City city)
        {
            Cities.Add(city);
        }

        public void AddRoute(Route route)
        {
            AddCity(route.Origin);
            AddCity(route.Destination);
            Edges.Add(new Edge(route.Origin, route.Destination, route.Length));
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

        public (int, List<Edge>) LongestContinuousPath()
        {
            int longestPathLength = 0;
            List<Edge> longestPathEdges = new List<Edge>();

            foreach (var city in Cities)
            {
                var visitedEdges = new HashSet<Edge>();
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
    }
    public class Edge
    {
        public City Origin { get; set; }
        public City Destination { get; set; }
        public int Cost { get; set; }

        public Edge(City origin, City destination, int cost)
        {
            Origin = origin;
            Destination = destination;
            Cost = cost;
        }
    }

}

