﻿using Microsoft.AspNetCore.Mvc.TagHelpers;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public class EvaluationBasedBot : BotPlayer
    {
        protected RouteGraph GameRouteGraph { get; set; }

        private List<GameBoard.Route> bestPath { get; set; } = new List<GameBoard.Route>();

        private int totalPathPoints { get; set; }

        private Dictionary<TrainColor, int> numberOfCardsNeededOfEachColor { get; set; }

        public EvaluationBasedBot(string name, PlayerColor color, int index, RouteGraph routeGraph) : base(name, color, index)
        {
            GameRouteGraph = routeGraph;
        }

        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
        {
            var playerCards = GetGroupedTrainColors();

            if (possibleMoves.ChooseDestinationCardMoves.Count > 0)
            {
                var (move, combinedPath, Cost) = ChooseBestDestinationCardsCombination(possibleMoves.ChooseDestinationCardMoves, playerCards, game.GameTurn == 1);
                bestPath = combinedPath;
                UpdateNeededTrainCards(bestPath, playerCards);
                return move;
            }

            if (bestPath.Count != 0 && !IsPathStillAvailabe(bestPath, game.Board.RouteGraph))
            {
                //compute new best route 
                bestPath = ComputeNewBestPath(PendingDestinationCards, playerCards);
            }

            if (bestPath.Count > 0)
            {
                UpdateNeededTrainCards(bestPath, playerCards);
            }

            var evaluatedMoves = EvaluateAllPossibleMoves(possibleMoves, playerCards);
            return evaluatedMoves.First().move;
        }

        private List<(Move move, double points)> EvaluateAllPossibleMoves(PossibleMoves possibleMoves, Dictionary<TrainColor, int> playerCards)
        {
            var listOfMovesAndPoints = new List<(Move move, double points)>();

            if (possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                foreach (var move in possibleMoves.DrawTrainCardMoves)
                {
                    var points = EvaluateDrawTrainCardMove(move);
                    listOfMovesAndPoints.Add((move, points));
                }
            }

            if (possibleMoves.ClaimRouteMoves.Count > 0)
            {
                foreach (var move in possibleMoves.ClaimRouteMoves)
                {
                    var points = EvaluateClaimRouteMove(move, playerCards);
                    listOfMovesAndPoints.Add((move, points));
                }
            }

            if (possibleMoves.DrawDestinationCardMove != null)
            {
                var points = EvaluateDrawDestinationCardMove(possibleMoves.DrawDestinationCardMove);
                listOfMovesAndPoints.Add((possibleMoves.DrawDestinationCardMove, points));
            }

            return listOfMovesAndPoints.OrderByDescending(m => m.points).ToList();
        }

        private double EvaluateDrawTrainCardMove(DrawTrainCardMove move)
        {
            //hidden card
            if (move.faceUpCardIndex == -1)
            {
                return 1.0;
            }

            //visible train card
            var cardColor = move.CardColor;

            //locomotives get 2 points
            if (cardColor == TrainColor.Locomotive)
            {
                return 2.0;
            }

            //other cards get nrOfCardsNeededOfThatColor points
            if (numberOfCardsNeededOfEachColor.ContainsKey(cardColor))
            {
                var neededCards = numberOfCardsNeededOfEachColor[cardColor];
                if (neededCards > 0)
                {
                    return neededCards;
                }
            }

            return 0.0;
        }

        private double EvaluateClaimRouteMove(ClaimRouteMove move, Dictionary<TrainColor, int> playerCards)
        {
            var points = 0.0;

            //first, add the points relating to the route selection

            //if the player still has destination cards to complete
            if (PendingDestinationCards.Count > 0)
            {
                //if part of path
                if (bestPath.Contains(move.Route.ElementAt(0)))
                {
                    points += 1.0;
                    var routePoints = move.Route.ElementAt(0).PointValue;

                    points = 2 + routePoints + totalPathPoints - ComputeUnbuildRoutePointsFromPath();
                }
                //else , if not part of path
                else
                {
                    points += -1.0;
                }
            }

            //if the player does not have any destination cards to complete
            else
            {
                points += (double)move.Route.ElementAt(0).PointValue;
            }

            //then, add the points relating to with which cards the route will be built
            points += EvaluateBuildRouteMove(move, playerCards);

            return points;
        }

        private int EvaluateBuildRouteMove(ClaimRouteMove move, Dictionary<TrainColor, int> playerCards)
        {
            //for grey routes
            //get the number of cards of that color which the player has
            var routeColor = move.Route.ElementAt(0).Color;

            var numberOfCardPlayerHasOfColor = 0;
            var numberOfLocomotives = 0;
            var numberOfLocomotivesUsed = 0;

            if (playerCards.ContainsKey(TrainColor.Locomotive))
            {
                numberOfLocomotives = playerCards[TrainColor.Locomotive];
            }

            if (playerCards.ContainsKey(routeColor))
            {
                numberOfCardPlayerHasOfColor = playerCards[routeColor];
            }
            else
            {
                numberOfCardPlayerHasOfColor = 0;
            }

            if (numberOfCardPlayerHasOfColor < move.Route.ElementAt(0).Length)
            {
                //the rest of the cards used are locomotives
                numberOfLocomotivesUsed = move.Route.ElementAt(0).Length - numberOfCardPlayerHasOfColor;
            }

            //if wild cards are used
            if (numberOfLocomotivesUsed > 0)
            {
                return -9 * numberOfLocomotivesUsed;
            }

            //for gray routes, return -cardsFromThatColorNeededForPathRoutes
            if (routeColor == TrainColor.Grey)
            {
                return -numberOfCardsNeededOfEachColor[move.ColorUsed];
            }

            //else, return 0
            return 0;
        }

        private double EvaluateDrawDestinationCardMove(DrawDestinationCardMove move)
        {
            if (PendingDestinationCards.Count > 0)
            {
                return -1.0;
            }

            return -15.0 + RemainingTrains;
        }

        private int ComputeUnbuildRoutePointsFromPath()
        {
            var points = 0;
            foreach (var route in bestPath)
            {
                if (!route.CanPlayerUseRoute(Color))
                {
                    points += route.PointValue;
                }
            }
            return points;
        }
        private List<GameBoard.Route> ComputeNewBestPath(List<DestinationCard> destinationCards, Dictionary<TrainColor, int> playerCards)
        {
            List<(List<GameBoard.Route> CombinedPath, double Cost, int TotalPoints)> ticketCombinations = new List<(List<GameBoard.Route>, double, int)>();
            var validPathsToCompleteTickets = GetAllValidPathsBetweenDestinations(destinationCards);

            // Evaluate the cost for each validPathsToCompleteTickets and choose the one with the smallest cost
            foreach (var pathCombination in validPathsToCompleteTickets)
            {
                double cost = CalculateCombinedPathCost(pathCombination.Path, playerCards) - pathCombination.Points * 0.5;
                ticketCombinations.Add((pathCombination.Path, cost, pathCombination.Points));
            }

            var (CombinedPath, Cost, TotalPoints) = ticketCombinations.OrderBy(t => t.Cost).FirstOrDefault();
            totalPathPoints = TotalPoints;
            return CombinedPath;
        }

        private List<(List<GameBoard.Route> Path, int Points)> GetAllValidPathsBetweenDestinations(List<DestinationCard> destinationCards)
        {
            //foreach 2 cities from the destination cards, computes those paths between them
            //then, computes unions and return valid paths for reaching all the cities on the destination cards
            Dictionary<DestinationCard, List<(List<GameBoard.Route> Path, int Points)>> pathsPerTicket = new Dictionary<DestinationCard, List<(List<GameBoard.Route>, int)>>();

            foreach (var ticket in destinationCards)
            {
                var paths = GameRouteGraph.FindAllPaths(ticket.Origin, ticket.Destination, false);
                if (paths.Any())
                {
                    List<(List<GameBoard.Route>, int)> pathsForTicket = new List<(List<GameBoard.Route>, int)>();
                    foreach (var path in paths)
                    {
                        int totalPoints = ticket.PointValue + path.Sum(route => route.PointValue);
                        pathsForTicket.Add((path, totalPoints));
                    }
                    pathsPerTicket.Add(ticket, pathsForTicket);
                }
            }

            // Generate all combinations of paths for the current combination of destination cards
            var allUnionPaths = GenerateAllValidPathCombinations(pathsPerTicket, destinationCards.Count);

            //discard the ones that take more than 45 cards
            var validPaths = allUnionPaths.Where(p => p.Path.Sum(r => r.Length) <= 45).ToList();

            // Sort the remaining combinations by the shortest path length
            validPaths.Sort((a, b) => a.Path.Sum(r => r.Length).CompareTo(b.Path.Sum(r => r.Length)));

            return validPaths;
        }

        private (ChooseDestinationCardMove move, List<GameBoard.Route> CombinedPath, double Cost) ChooseBestDestinationCardsCombination(
            List<ChooseDestinationCardMove> possibleMoves,
            Dictionary<TrainColor, int> playerTrainCards,
            bool isFirstTurn)
        {
            List<(ChooseDestinationCardMove move, List<GameBoard.Route> CombinedPath, double Cost, int TotalPoints)> ticketCombinations = new List<(ChooseDestinationCardMove, List<GameBoard.Route>, double, int)>();

            var allCards = possibleMoves
                  .First(c => c.ChosenDestinationCards.Count == 3)
                  .ChosenDestinationCards;

            foreach (var combination in possibleMoves)
            {
                var validPaths = GetAllValidPathsBetweenDestinations(combination.ChosenDestinationCards);

                if (validPaths.Count() == 0)
                {
                    int cost = -1000;

                    ticketCombinations.Add((combination, new List<GameBoard.Route>(), cost, cost));
                }
                else
                {
                    // Evaluate the cost for each combination and choose the one with the smallest cost
                    foreach (var pathCombination in validPaths)
                    {
                        double cost = CalculateCombinedPathCost(pathCombination.Path, playerTrainCards) - pathCombination.Points * 0.5;
                        cost = -1 * cost;
                        ticketCombinations.Add((combination, pathCombination.Path, cost, pathCombination.Points));
                    }
                }

            }

            var (move, CombinedPath, Cost, TotalPoints) = ticketCombinations.OrderBy(t => t.Cost).FirstOrDefault();
            totalPathPoints = TotalPoints;
            return (move, CombinedPath, Cost);
        }

        private double CalculateCombinedPathCost(List<GameBoard.Route> combinedPath, Dictionary<TrainColor, int> playerTrainCards)
        {
            double cost = 0;

            var groupedByColor = combinedPath.GroupBy(r => r.Color);

            foreach (var group in groupedByColor)
            {
                var color = group.Key;
                int length = group.Sum(r => r.Length);

                if (color == TrainColor.Grey)
                {
                    // Routes without certain color
                    cost += length;
                }
                else
                {
                    // Color-specific routes
                    if (playerTrainCards.ContainsKey(color))
                    {
                        var remainingCardsNeeded = playerTrainCards[color];
                        //if we have more cards than needed, cost is 0
                        if (remainingCardsNeeded > length)
                        {
                            cost += 0;
                        }
                        else
                        {
                            //we need more cards
                            remainingCardsNeeded = length - remainingCardsNeeded;
                            cost += Math.Pow(remainingCardsNeeded, 2);
                        }
                    }
                }
            }

            return cost;
        }

        private List<(List<GameBoard.Route> Path, int Points)> GenerateAllValidPathCombinations(Dictionary<DestinationCard, List<(List<GameBoard.Route> Path, int Points)>> pathsPerTicket, int numDestinationCards)
        {
            List<(List<GameBoard.Route> Path, int Points)> validCombinations = new List<(List<GameBoard.Route>, int)>();

            // Generate combinations recursively
            GeneratePathCombinations(pathsPerTicket, numDestinationCards, new List<(List<GameBoard.Route>, int)>(), 0, validCombinations);

            return validCombinations;
        }

        private void GeneratePathCombinations(Dictionary<DestinationCard, List<(List<GameBoard.Route> Path, int Points)>> pathsPerTicket, int numDestinationCards, List<(List<GameBoard.Route> Path, int Points)> currentCombination, int currentCardIndex, List<(List<GameBoard.Route> Path, int Points)> validCombinations)
        {
            // Base case: If the current combination contains paths from all destination cards, add it to the list of valid combinations
            if (currentCombination.Count == numDestinationCards)
            {
                // Flatten the list of paths and calculate the total points
                var combinedPath = currentCombination.SelectMany(x => x.Path).ToList();
                var totalPoints = currentCombination.Sum(x => x.Points);

                validCombinations.Add((combinedPath, totalPoints));
                return;
            }

            // Ensure the currentCardIndex is within the bounds of pathsPerTicket
            if (currentCardIndex >= pathsPerTicket.Count)
            {
                return;
            }

            // Get the current destination ticket and its paths
            var currentTicket = pathsPerTicket.Keys.ElementAt(currentCardIndex);
            var ticketPaths = pathsPerTicket[currentTicket];

            // Iterate through all paths for the current destination card
            foreach (var path in ticketPaths)
            {
                // Create a new combination list by adding the current path
                var newPathList = new List<(List<GameBoard.Route>, int)>(currentCombination) { path };

                // Recursively generate combinations with the new path added
                GeneratePathCombinations(pathsPerTicket, numDestinationCards, newPathList, currentCardIndex + 1, validCombinations);
            }
        }

        private bool IsPathStillAvailabe(List<GameBoard.Route> bestPath, RouteGraph gameRoutes)
        {
            return gameRoutes.IsPathAvailableToPlayer(bestPath, Color);
        }

        private void UpdateNeededTrainCards(List<GameBoard.Route> bestPath, Dictionary<TrainColor, int> playerCards)
        {
            numberOfCardsNeededOfEachColor = new Dictionary<TrainColor, int>();

            foreach (var route in bestPath)
            {
                if (route.Color == TrainColor.Grey)
                {
                    continue;
                }

                int neededCards = route.Length;

                if (playerCards.ContainsKey(route.Color))
                {
                    neededCards -= playerCards[route.Color];
                }

                // Ensure neededCards is not negative
                neededCards = Math.Max(0, neededCards);

                if (numberOfCardsNeededOfEachColor.ContainsKey(route.Color))
                {
                    numberOfCardsNeededOfEachColor[route.Color] += neededCards;
                }
                else
                {
                    numberOfCardsNeededOfEachColor.Add(route.Color, neededCards);
                }
            }
        }
    }
}
