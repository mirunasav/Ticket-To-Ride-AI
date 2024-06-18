using System.Collections.Generic;
using TicketToRide.Helpers;
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

        private List<(List<GameBoard.Route> Path, int Points)> bestPathsCollection { get; set; } = new List<(List<GameBoard.Route>, int points)>();

        private int totalPathPoints { get; set; }

        private Dictionary<TrainColor, int> numberOfCardsNeededOfEachColor { get; set; }

        private int numberOfPlayers { get; set; } = 0;


        public EvaluationBasedBot(string name, PlayerColor color, int index, RouteGraph routeGraph, int numberOfPlayers) : base(name, color, index)
        {
            GameRouteGraph = routeGraph;
            this.numberOfPlayers = numberOfPlayers;
        }

        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
        {
            var playerCards = GetGroupedTrainColors();

            if (possibleMoves.ChooseDestinationCardMoves.Count > 0)
            {
                var (move, combinedPath, Cost) = ChooseBestDestinationCardsCombination(possibleMoves.ChooseDestinationCardMoves, playerCards, game.GameTurn == 1, game.Players.Count);
                bestPath = combinedPath;
                UpdateNeededTrainCards(bestPath, playerCards);
                return move;
            }

            if (bestPath.Count != 0)
            {
                //If the best path is no longer available a new best path
                //and other possible paths are found
                if (!IsPathStillAvailabe(bestPath, game.Board.RouteGraph))
                {
                    //compute new best path and path collections
                    ComputeNewBestPath(PendingDestinationCards, playerCards, game.Players.Count);
                    if (bestPath.Count > 0)
                    {
                        UpdateNeededTrainCards(bestPath, playerCards);
                    }
                }
                else
                {
                    //If it is available, all paths that can complete the tickets are
                    //re evaluated based on the cards in player’s hand
                    ChooseBestPath(playerCards, game.Players.Count);
                }

            }

            var evaluatedMoves = EvaluateAllPossibleMoves(possibleMoves, playerCards, game.Players, game.GameTurn);
            var bestMove = evaluatedMoves.First().move;

            if (bestMove is ClaimRouteMove bestClaimRouteMove)
            {
                // Filter ClaimRouteMoves with the same origin and destination as the best move
                var matchingClaimRouteMoves = evaluatedMoves
                    .Where(e => e.move is ClaimRouteMove claimRouteMove &&
                                claimRouteMove.Origin == bestClaimRouteMove.Origin &&
                                claimRouteMove.Destination == bestClaimRouteMove.Destination)
                    .OrderByDescending(e => e.secondaryPoints)
                    .ToList();

                // Assuming you want to select the move with the highest secondary points
                var selectedBestClaimRouteMove = matchingClaimRouteMoves.FirstOrDefault();

                return selectedBestClaimRouteMove.move;
            }

            return bestMove;
        }

        private List<(Move move, double points, double secondaryPoints)> EvaluateAllPossibleMoves(
            PossibleMoves possibleMoves,
            Dictionary<TrainColor, int> playerCards,
            List<Player> players,
            int gameTurn)
        {
            var listOfMovesAndPoints = new List<(Move move, double points, double secondaryPoints)>();

            if (possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                foreach (var move in possibleMoves.DrawTrainCardMoves)
                {
                    var points = EvaluateDrawTrainCardMove(move);
                    listOfMovesAndPoints.Add((move, points, 0.0));
                }
            }

            if (possibleMoves.ClaimRouteMoves.Count > 0)
            {
                foreach (var move in possibleMoves.ClaimRouteMoves)
                {
                    (var points, var secondaryPoints) = EvaluateClaimRouteMove(move, playerCards, numberOfPlayers);
                    listOfMovesAndPoints.Add((move, points, secondaryPoints));
                }
            }

            if (possibleMoves.DrawDestinationCardMove != null)
            {
                var points = EvaluateDrawDestinationCardMove(possibleMoves.DrawDestinationCardMove, players, gameTurn);
                listOfMovesAndPoints.Add((possibleMoves.DrawDestinationCardMove, points, 0.0));
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

        private (double points, double secondaryPoints) EvaluateClaimRouteMove(ClaimRouteMove move, Dictionary<TrainColor, int> playerCards, int numberOfPlayers)
        {
            //points : points for selecting that route ;
            //secondaryPoints: points for how the route will be built
            var points = 0.0;
            var secondaryPoints = 0.0;
            var priority = 0;

            if (move.Route.ElementAt(0).Length <= 3)
            {
                priority += 6 - move.Route.ElementAt(0).Length;
            }
            if (move.Route.ElementAt(0).Color == TrainColor.Grey)
            {
                priority += 2;
            }
            //var loss = GetLossIfRouteIsNotClaimed(move.Route.ElementAt(0), playerCards, numberOfPlayers);

            //priority += (int)loss;
            //first, add the points relating to the route selection

            //if the player still has destination cards to complete
            if (PendingDestinationCards.Count > 0)
            {
                //if part of path
                if (bestPath.Contains(move.Route.ElementAt(0)))
                {
                    var equivalentClaimedRoute = ClaimedRoutes.GetEquivalentEdge(move.Route.ElementAt(0));
                    if (equivalentClaimedRoute == null || !equivalentClaimedRoute.Routes.Any(r => r.ClaimedBy == Color))
                    {
                        //route is not claimed
                        var routePoints = move.Route.ElementAt(0).PointValue;

                        points = 5 + routePoints * 2 + totalPathPoints - ComputeUnbuildRoutePointsFromPath() + priority;
                    }
                    //var isClaimed = bestPath.First(r => r.Equals(move.Route.ElementAt(0))).IsClaimed;
                    else
                    {
                        points += -100.0; //dont build a route which is already claimed 
                    }
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
            secondaryPoints += EvaluateBuildRouteMove(move, playerCards);

            return (points, secondaryPoints);
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

        private double EvaluateDrawDestinationCardMove(DrawDestinationCardMove move, List<Player> players, int gameTurn)
        {
            if (PendingDestinationCards.Count > 0)
            {
                return -1.0;
            }

            var points = -10.0 + RemainingTrains;

            foreach (var player in players)
            {
                if (player.RemainingTrains < 10)
                {
                    points -= 10.0;
                }

                if (players.Count == 2 && gameTurn > 40)
                {
                    points -= 10;
                }

                if (players.Count >= 3 && gameTurn > 30)
                {
                    points -= 10;
                }
            }

            return points;
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

        private void ComputeNewBestPath(List<DestinationCard> destinationCards, Dictionary<TrainColor, int> playerCards, int numberOfPlayers)
        {
            var validPathsToCompleteTickets = GetAllValidPathsBetweenDestinations(destinationCards, numberOfPlayers, RemainingTrains);

            if(validPathsToCompleteTickets.Count == 0)
            {
               ChooseRemainingDestinationCards(playerCards);
               UpdateNeededTrainCards(bestPath, playerCards);
            }

            bestPathsCollection = validPathsToCompleteTickets.Select(v => (v.Path, v.Points)).ToList();

            // Evaluate the cost for each validPathsToCompleteTickets and choose the one with the smallest cost
            ChooseBestPath(playerCards, numberOfPlayers);
        }

        private void ChooseBestPath(Dictionary<TrainColor, int> playerCards, int numberOfPlayers)
        {
            List<(ChooseDestinationCardMove ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)> pathsPerCombination =
               new List<(ChooseDestinationCardMove ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)>();

            (ChooseDestinationCardMove ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost) bestChoice;

            // Evaluate the cost for each validPathsToCompleteTickets and choose the one with the smallest cost
            double pointsForLowestRoute = 0;
            double lowestCost = int.MaxValue;

            foreach (var pathCombination in bestPathsCollection)
            {
                double cost = CalculateCombinedPathCost(pathCombination.Path, playerCards);// -  pathCombination.Points * 0.5;
                if (cost < lowestCost)
                {
                    lowestCost = cost;
                    bestPath = pathCombination.Path;
                    pointsForLowestRoute = pathCombination.Points;
                }
            }
            totalPathPoints = (int)pointsForLowestRoute;
        }

        private List<(List<GameBoard.Route> Path, int Points, int Length)> GetAllValidPathsBetweenDestinations(List<DestinationCard> destinationCards, int numberOfPlayers, int maxLength, int maxPaths = 100)
        {
            //foreach 2 cities from the destination cards, computes those paths between them
            //then, computes unions and return valid paths for reaching all the cities on the destination cards
            Dictionary<DestinationCard, List<(List<GameBoard.Route> Path, int Points)>> pathsPerTicket = new Dictionary<DestinationCard, List<(List<GameBoard.Route>, int)>>();

            foreach (var ticket in destinationCards)
            {
                var paths = GameRouteGraph.FindAllPaths(ticket.Origin, ticket.Destination, false, Color, 100, numberOfPlayers);
                if (paths.Any())
                {
                    List<(List<GameBoard.Route>, int)> pathsForTicket = new List<(List<GameBoard.Route>, int)>();
                    foreach (var path in paths)
                    {
                        int totalPoints = path.Sum(route => route.PointValue);
                        pathsForTicket.Add((path, totalPoints));
                    }
                    pathsPerTicket.Add(ticket, pathsForTicket);
                }
            }

            // Generate all combinations of paths for the current combination of destination cards
            var allUnionPaths = GenerateAllValidPathCombinations(pathsPerTicket, destinationCards.Count);

            //when generating a new path, discard the ones that are too long
            var maxPathLength = RemainingTrains == 45 ? 35 : RemainingTrains;

            allUnionPaths = allUnionPaths.OrderBy(a => a.length).ToList();

            var validPaths = allUnionPaths.Where(p => p.length <= RemainingTrains).ToList();

            if (validPaths.Count == 0)
            {
                //try to only achieve one detination ticket intead
                return validPaths;

            }
            // Sort the remaining combinations by the shortest path length
            validPaths.Sort((a, b) => a.Path.Sum(r => r.Length).CompareTo(b.Path.Sum(r => r.Length)));

            return validPaths;
        }

        private (ChooseDestinationCardMove move, List<GameBoard.Route> CombinedPath, double Cost) ChooseBestDestinationCardsCombination(
            List<ChooseDestinationCardMove> possibleMoves,
            Dictionary<TrainColor, int> playerTrainCards,
            bool isFirstTurn,
            int numberOfPlayers)
        {
            List<(ChooseDestinationCardMove ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)> pathsPerCombination =
                new List<(ChooseDestinationCardMove ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)>();

            (ChooseDestinationCardMove ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost) bestChoice;

            var allCards = possibleMoves
                  .First(c => c.ChosenDestinationCards.Count == 3)
                  .ChosenDestinationCards;

            foreach (var combination in possibleMoves)
            {
                var validPaths = GetAllValidPathsBetweenDestinations(combination.ChosenDestinationCards, numberOfPlayers, 35);
                // bestPathsCollection = validPaths;
                double lowestCost = int.MaxValue;
                int pointsForLowestRoute = 0;
                List<GameBoard.Route> lowestCostPath = new List<GameBoard.Route>();

                if (validPaths.Count() == 0)
                {
                    int cost = 1000;
                    lowestCost = cost;
                    pathsPerCombination.Add((combination, new List<(List<GameBoard.Route>, int)>(), new List<GameBoard.Route>(), cost, 0));
                }
                else
                {
                    // Evaluate the cost for each combination and choose the one with the smallest cost
                    List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints = new List<(List<GameBoard.Route>, int)>();

                    foreach (var pathCombination in validPaths)
                    {
                        double cost = CalculateCombinedPathCost(pathCombination.Path, playerTrainCards);//- pathCombination.Points * 0.5;// + pathCombination.Path.Sum(r => r.Length);
                        if (cost < lowestCost)
                        {
                            lowestCost = cost;
                            lowestCostPath = pathCombination.Path;
                            pointsForLowestRoute = pathCombination.Points;
                        }
                        validPathsAndPoints.Add((pathCombination.Path, (int)cost));
                    }
                    pathsPerCombination.Add((combination, validPathsAndPoints, lowestCostPath, lowestCost, pointsForLowestRoute));
                }
            }

            bestChoice = (pathsPerCombination.First().ticketCombination, pathsPerCombination.First().validPathsAndPoints, pathsPerCombination.First().lowestCostPath, pathsPerCombination.First().lowestCost);
            
            double lowestPathCost = int.MaxValue;
            foreach (var combination in pathsPerCombination)
            {
                var pointsForCompletingPathAndDestination = (combination.pointsForLowestRoute + combination.ticketCombination.ChosenDestinationCards.Sum(c => c.PointValue)) * 0.5;
                var costOfPath = combination.lowestCost - pointsForCompletingPathAndDestination;
                if (costOfPath < lowestPathCost)
                {
                    lowestPathCost = costOfPath;
                    bestChoice = (combination.ticketCombination, combination.validPathsAndPoints, combination.lowestCostPath, lowestPathCost);
                }
            }

            bestPathsCollection = bestChoice.validPathsAndPoints;
            //valid paths => remain the ones form the last combination; they should be the ones for the chosen combination
            //(combination, points, valid paths) => for each combination get lowest cost for each combination => compare with the other combinations=> cost - points * 0.5 including dest ticket value

            return (bestChoice.ticketCombination, bestChoice.lowestCostPath, bestChoice.lowestCost);
        }

        private double CalculateCombinedPathCost(List<GameBoard.Route> combinedPath, Dictionary<TrainColor, int> playerTrainCards)
        {
            ////calculate points/ trains
            var remainingPathToBeClaimed = combinedPath.Where(r => !(r.IsClaimed && r.ClaimedBy == Color)).ToList();

            double cost = 0;

            var groupedByColor = remainingPathToBeClaimed.GroupBy(r => r.Color);

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
                        //if we have more cards than needed, cost is length
                        if (remainingCardsNeeded > length)
                        {
                            cost += length;
                        }
                        else
                        {
                            //we need more cards
                            remainingCardsNeeded = length - remainingCardsNeeded;
                            cost += Math.Pow(remainingCardsNeeded, 2);
                        }
                    }
                    else
                    {
                        cost += Math.Pow(length, 2);
                    }
                }
            }

            return cost;
        }

        private List<(List<GameBoard.Route> Path, int Points, int length)> GenerateAllValidPathCombinations(Dictionary<DestinationCard, List<(List<GameBoard.Route> Path, int Points)>> pathsPerTicket, int numDestinationCards)
        {
            List<(List<GameBoard.Route> Path, int Points, int length)> validCombinations = new List<(List<GameBoard.Route>, int, int)>();

            // Generate combinations recursively
            GeneratePathCombinations(pathsPerTicket, numDestinationCards, new List<(List<GameBoard.Route>, int)>(), 0, validCombinations);

            return validCombinations;
        }

        private void GeneratePathCombinations(Dictionary<DestinationCard, List<(List<GameBoard.Route> Path, int Points)>> pathsPerTicket, int numDestinationCards, List<(List<GameBoard.Route> Path, int Points)> currentCombination, int currentCardIndex, List<(List<GameBoard.Route> Path, int Points, int Length)> validCombinations)
        {
            // Base case: If the current combination contains paths from all destination cards, add it to the list of valid combinations
            if (currentCombination.Count == numDestinationCards)
            {
                // Create a HashSet to track unique origin-destination pairs
                var routeSet = new HashSet<(City, City)>();
                var combinedPath = new List<GameBoard.Route>();

                // Flatten the list of paths and add only unique routes based on origin-destination
                foreach (var route in currentCombination.SelectMany(x => x.Path))
                {
                    // Create a tuple for the route's origin-destination pair
                    var routePair = (route.Origin, route.Destination);

                    // Add route to the combinedPath if it's unique
                    if (!routeSet.Contains(routePair))
                    {
                        combinedPath.Add(route);
                        routeSet.Add(routePair);
                    }
                }

                // Calculate the total points
                var totalPoints = currentCombination.Sum(x => x.Points);
                var length = combinedPath.Where(r => !r.CanPlayerUseRoute(Color)).Sum(r => r.Length);
                validCombinations.Add((combinedPath, totalPoints, length));
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

        private double GetLossIfRouteIsNotClaimed(GameBoard.Route route, Dictionary<TrainColor, int> playerCards, int numberOfPlayers)
        {
            (int distance, var path) = GameRouteGraph.FindShortestPathWithoutRoute(route.Origin, route.Destination, Color, numberOfPlayers);

            var routesInShortestPaths = new List<GameBoard.Route>();
            routesInShortestPaths.AddRange(path.SelectMany(e => e.Routes));

            var color = route.Color;
            var cost = CalculateCombinedPathCost(routesInShortestPaths, playerCards);
            return cost;
        }

        private (List<DestinationCard> ticketCombination, List<GameBoard.Route> lowestCostPath) ChooseRemainingDestinationCards(Dictionary<TrainColor, int> playerCards)
        {
            List<(List<DestinationCard> destinationCards, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)> pathsPerCombination =
            new List<(List<DestinationCard> destinationCards, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)>();

            (List<DestinationCard> ticketCombination, List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints, List<GameBoard.Route> lowestCostPath, double lowestCost) bestChoice;
            var combinations = CombinationGenerator.GetCombinations(PendingDestinationCards, 1, PendingDestinationCards.Count);

            foreach (var combination in combinations)
            {
                var validPaths = GetAllValidPathsBetweenDestinations(combination, numberOfPlayers, 35);
                // bestPathsCollection = validPaths;
                double lowestCost = int.MaxValue;
                int pointsForLowestRoute = 0;
                List<GameBoard.Route> lowestCostPath = new List<GameBoard.Route>();

                if (validPaths.Count() == 0)
                {
                    int cost = 1000;
                    lowestCost = cost;
                    pathsPerCombination.Add((combination, new List<(List<GameBoard.Route>, int)>(), new List<GameBoard.Route>(), cost, 0));
                }
                else
                {
                    // Evaluate the cost for each combination and choose the one with the smallest cost
                    List<(List<GameBoard.Route> validPath, int costOfPath)> validPathsAndPoints = new List<(List<GameBoard.Route>, int)>();

                    foreach (var pathCombination in validPaths)
                    {
                        double cost = CalculateCombinedPathCost(pathCombination.Path, playerCards);//- pathCombination.Points * 0.5;// + pathCombination.Path.Sum(r => r.Length);
                        if (cost < lowestCost)
                        {
                            lowestCost = cost;
                            lowestCostPath = pathCombination.Path;
                            pointsForLowestRoute = pathCombination.Points;
                        }
                        validPathsAndPoints.Add((pathCombination.Path, (int)cost));
                    }
                    pathsPerCombination.Add((combination, validPathsAndPoints, lowestCostPath, lowestCost, pointsForLowestRoute));
                }
            }

            bestChoice = (pathsPerCombination.First().destinationCards, pathsPerCombination.First().validPathsAndPoints, pathsPerCombination.First().lowestCostPath, pathsPerCombination.First().lowestCost);

            double lowestPathCost = int.MaxValue;
            foreach (var combination in pathsPerCombination)
            {
                var pointsForCompletingPathAndDestination = (combination.pointsForLowestRoute + combination.destinationCards.Sum(c => c.PointValue)) * 0.5;
                var costOfPath = combination.lowestCost - pointsForCompletingPathAndDestination;
                if (costOfPath < lowestPathCost)
                {
                    lowestPathCost = costOfPath;
                    bestChoice = (combination.destinationCards, combination.validPathsAndPoints, combination.lowestCostPath, lowestPathCost);
                }
            }

            bestPathsCollection = bestChoice.validPathsAndPoints;
            bestPath = bestChoice.lowestCostPath;
            //valid paths => remain the ones form the last combination; they should be the ones for the chosen combination
            //(combination, points, valid paths) => for each combination get lowest cost for each combination => compare with the other combinations=> cost - points * 0.5 including dest ticket value

            return (bestChoice.ticketCombination, bestChoice.lowestCostPath);
        }
    }
}
