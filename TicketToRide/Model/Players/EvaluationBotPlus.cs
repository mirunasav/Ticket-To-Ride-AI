//using TicketToRide.Model.Enums;
//using TicketToRide.Model.GameBoard;
//using TicketToRide.Moves;

//namespace TicketToRide.Model.Players
//{
//    public class LongestRouteBotPlus : SimpleStrategyBot
//    {
//        public LongestRouteBotPlus(string name, PlayerColor color, int index, RouteGraph routeGraph) : base(name, color, index, routeGraph) 
//        {
//        }

//        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
//        {
//            Random random = new Random();
//            int randomIndex;

//            if (possibleMoves.ChooseDestinationCardMoves.Count > 0)
//            {
//                ChooseDestinationCardMove bestMove;

//                var allCards = possibleMoves
//                    .ChooseDestinationCardMoves
//                    .First(c => c.ChosenDestinationCards.Count == 3)
//                    .ChosenDestinationCards;

//                //choose the one with the biggest points and least needed amount of trains to finish
//                if (possibleMoves.ChooseDestinationCardMoves.Exists(c => c.ChosenDestinationCards.Count == 1))
//                {
//                    var bestCard = GetBestDestinationCard(allCards, game);

//                    bestMove = possibleMoves.ChooseDestinationCardMoves
//                        .Where(c => c.ChosenDestinationCards.Count == 1
//                         && c.ChosenDestinationCards.Contains(bestCard)).First();

//                    return bestMove;
//                }

//                //else, choose the one with the most points and the one with the least points to minimise loss

//                var bestPointsCard = allCards.OrderByDescending(c => c.PointValue).First();
//                var leastPointsCard = allCards.OrderBy(c => c.PointValue).First();

//                var bestMoveQuery = possibleMoves.ChooseDestinationCardMoves
//                    .Where(c => c.ChosenDestinationCards.Count == 2 &&
//                    c.ChosenDestinationCards.Contains(bestPointsCard));

//                if (leastPointsCard.PointValue <= 11)
//                {
//                    bestMoveQuery = bestMoveQuery.Where(c => c.ChosenDestinationCards.Contains(leastPointsCard));
//                }

//                bestMove = bestMoveQuery.First();

//                return bestMove;
//            }

//            List<GameBoard.Route> shortestPathRoutes = [];

//            var allDestinationCards = CompletedDestinationCards.Union(PendingDestinationCards).ToList();

//            var areAllCitiesConnected = ClaimedRoutes.AreAllCitiesConnected(allDestinationCards);

//            if (!areAllCitiesConnected)
//            {
//                shortestPathRoutes = GameRouteGraph.GetRemainingRoutesToConnectAllCities(allDestinationCards, Color, game.Board.Routes, game.Players.Count);
//            }

//            if (possibleMoves.ClaimRouteMoves.Count > 0)
//            {
//                //choose routes which are on one of the shortest paths
//                //between the cities which need to be connected

//                if (!(shortestPathRoutes is null || shortestPathRoutes.Count == 0))
//                {
//                    //check if the 
//                    //see if there is a possible route which is in these 
//                    var possibleClaimRouteMoves = new List<ClaimRouteMove>();

//                    foreach (var possibleClaimRouteMove in possibleMoves.ClaimRouteMoves)
//                    {

//                        var equalRoutes = shortestPathRoutes.Where(r =>
//                        r.Equals(possibleClaimRouteMove.Route.ElementAt(0))
//                        && !r.IsClaimed).ToList();

//                        if (equalRoutes.Count != 0)
//                        {
//                            possibleClaimRouteMoves.Add(possibleClaimRouteMove);
//                        }
//                    }

//                    if (possibleClaimRouteMoves.Count > 0)
//                    {
//                        var bestMove = GetBestClaimRouteMove(possibleClaimRouteMoves);
//                        return bestMove;
//                    }
//                }
//                else
//                {
//                    //the cities are unreachable so there is no goal to work to.
//                    //if the turn is less than 30 and the number of players is 2, get more tickets
//                    if (game.Players.Count < 3 && game.GameTurn <= 40)
//                    {
//                        return possibleMoves.DrawDestinationCardMove;
//                    }

//                    //otherwise, Simply claim a route, ordered by length, to bring most points
//                    //try to choose it to extend longest path
//                    //so as not to risk getting a card and not finishing it

//                    var claimRouteMoveQuery = possibleMoves.ClaimRouteMoves
//                        .OrderByDescending(r => r.Route.ElementAt(0).Length);

//                    var claimRouteMove = claimRouteMoveQuery
//                        .FirstOrDefault(r => DoesRouteExtendLongestPath(r.Route.ElementAt(0)));

//                    if (claimRouteMove == null)
//                    {
//                        claimRouteMove = claimRouteMoveQuery.First();
//                    }

//                    return claimRouteMove;
//                }
//            }

//            //if there are no routes which can be claimed and are helpful, try to draw train cards
//            if (possibleMoves.DrawTrainCardMoves.Count > 0)
//            {
//                //if there are paths that are useful to be claimed, figure out which train cards should
//                //be drawn in order to help
//                if (!(shortestPathRoutes is null || shortestPathRoutes.Count == 0))
//                {
//                    var neededTrainCards = GameRouteGraph.GetLeastTrainColorsNeededForShorthestPaths(shortestPathRoutes);
//                    var necessaryFaceUpCardExists = false;

//                    //check if a card of the needed color exists
//                    foreach (var drawTrainCardMove in possibleMoves.DrawTrainCardMoves)
//                    {
//                        if ((drawTrainCardMove.CardColor != default) &&
//                            (neededTrainCards.ContainsKey(drawTrainCardMove.CardColor)
//                            || drawTrainCardMove.CardColor == TrainColor.Locomotive
//                            || neededTrainCards.ContainsKey(TrainColor.Grey)))
//                        {
//                            necessaryFaceUpCardExists = true;
//                            break;
//                        }
//                    }

//                    if (!necessaryFaceUpCardExists)
//                    {
//                        //get face down deck card if the deck is not exhausted
//                        var faceDownDeckMove = possibleMoves.DrawTrainCardMoves.FirstOrDefault(move => move.faceUpCardIndex == -1);

//                        if (faceDownDeckMove != null)
//                        {
//                            return faceDownDeckMove;
//                        }

//                        //otherwise get random card from face up deck
//                        randomIndex = random.Next(0, possibleMoves.DrawTrainCardMoves.Count);
//                        return possibleMoves.DrawTrainCardMoves[randomIndex];
//                    }

//                    //if there are useful cards which can be drawn, figure out the most useful one
//                    var (neededColorsDictionary, mostFreqColor) = GetRemainingTrainColorsNeededForShorthestPaths(neededTrainCards);

//                    var availableTrainColors = new List<TrainColor>();
//                    DrawTrainCardMove bestMove = possibleMoves.DrawTrainCardMoves.ElementAt(0);
//                    int mostNeededCardCount = int.MaxValue;

//                    foreach (var drawTrainCardMove in possibleMoves.DrawTrainCardMoves)
//                    {
//                        if (mostFreqColor != default && neededColorsDictionary.ContainsKey(TrainColor.Grey))
//                        {
//                            if (mostFreqColor == drawTrainCardMove.CardColor && mostNeededCardCount < neededColorsDictionary[TrainColor.Grey])
//                            {
//                                mostNeededCardCount = neededColorsDictionary[TrainColor.Grey];
//                                bestMove = drawTrainCardMove;
//                            }
//                        }

//                        if (neededColorsDictionary.ContainsKey(drawTrainCardMove.CardColor))
//                        {
//                            if (neededColorsDictionary[drawTrainCardMove.CardColor] < mostNeededCardCount)
//                            {
//                                mostNeededCardCount = neededColorsDictionary[drawTrainCardMove.CardColor];
//                                bestMove = drawTrainCardMove;
//                            }
//                        }
//                    }

//                    return bestMove;
//                }

//                //otherwise, draw a random card
//                randomIndex = random.Next(0, possibleMoves.DrawTrainCardMoves.Count);
//                return possibleMoves.DrawTrainCardMoves[randomIndex];
//            }

//            //no useful draw train card moves or useful routes to be claimed so just claim randomly
//            if (possibleMoves.DrawTrainCardMoves.Count == 0 && possibleMoves.ClaimRouteMoves.Count > 0)
//            {
//                var claimRouteMove = possibleMoves.ClaimRouteMoves
//                    .OrderByDescending(r => r.Route.ElementAt(0).Length)
//                    .First();
//                return claimRouteMove;
//            }

//            //if there are not possible draw train card moves
//            return possibleMoves.DrawDestinationCardMove;
//        }

//        private bool DoesRouteExtendLongestPath(GameBoard.Route route)
//        {
//            (int pathLength, var longestContPath) = ClaimedRoutes.LongestContinuousPath();

//            ClaimedRoutes.AddRoute(route);

//            (int newPathLength, var newLongestContPath) = ClaimedRoutes.LongestContinuousPath();

//            ClaimedRoutes.RemoveRoute(route);

//            return newPathLength > pathLength;
//        }

//        private ClaimRouteMove GetBestClaimRouteMove (List<ClaimRouteMove> possibleClaimRouteMoves)
//        {
//            List<(ClaimRouteMove move, int priority)> listOfMovesAndPriorities = new List<(ClaimRouteMove move, int priority)> ();

//            foreach(var move in possibleClaimRouteMoves)
//            {
//                int priority = 0;
//                var route = move.Route.ElementAt(0);

//                priority += 6 - route.Length;

//                if (route.Color == TrainColor.Grey)
//                {
//                    priority += 2;
//                }
//                listOfMovesAndPriorities.Add((move, priority));
//            }

//            return listOfMovesAndPriorities.OrderByDescending(l => l.priority).First().move;
//        }

//    }
//}
using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public class EvaluationBotPlus : BotPlayer
    {
        protected RouteGraph GameRouteGraph { get; set; }

        private List<GameBoard.Route> bestPath { get; set; } = new List<GameBoard.Route>();

        private int totalPathPoints { get; set; }

        private Dictionary<TrainColor, int> numberOfCardsNeededOfEachColor { get; set; } = new Dictionary<TrainColor, int>();

        private int numberOfPlayers { get; set; } = 0;

        private int numberOfCompletedDestinationCards { get; set; } = 0;
        public EvaluationBotPlus(string name, PlayerColor color, int index, RouteGraph routeGraph, int numberOfPlayers) : base(name, color, index)
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

            //if the player still has destination cards to complete adn can complete them
            if (PendingDestinationCards.Count > 0 && bestPath.Count > 0)
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
            int points = 0;

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
                points -= 9 * numberOfLocomotivesUsed;
            }

            //for gray routes, return -cardsFromThatColorNeededForPathRoutes
            if (routeColor == TrainColor.Grey)
            {
                if (numberOfCardsNeededOfEachColor.ContainsKey(move.ColorUsed))
                {
                    points -= numberOfCardsNeededOfEachColor[move.ColorUsed];
                }
            }

            //else, return 0
            return points;
        }

        private double EvaluateDrawDestinationCardMove(DrawDestinationCardMove move, List<Player> players, int gameTurn)
        {
            if (PendingDestinationCards.Count > 0)
            {
                return -1000.0;
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
            var shortestPath = GameRouteGraph.FindAllShortestPathsBetweenDestinationCards(PendingDestinationCards, Color, numberOfPlayers);
            bestPath = shortestPath;
            totalPathPoints += shortestPath.Sum(r => r.PointValue);
        }

        private (ChooseDestinationCardMove move, List<GameBoard.Route> CombinedPath, double Cost) ChooseBestDestinationCardsCombination(
            List<ChooseDestinationCardMove> possibleMoves,
            Dictionary<TrainColor, int> playerTrainCards,
            bool isFirstTurn,
            int numberOfPlayers)
        {
            List<(ChooseDestinationCardMove ticketCombination, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)> pathsPerCombination =
                new List<(ChooseDestinationCardMove ticketCombination, List<GameBoard.Route> lowestCostPath, double lowestCost, int pointsForLowestRoute)>();

            (ChooseDestinationCardMove ticketCombination, List<GameBoard.Route> lowestCostPath, double lowestCost) bestChoice;

            var allCards = possibleMoves
                  .First(c => c.ChosenDestinationCards.Count == 3)
                  .ChosenDestinationCards;

            foreach (var combination in possibleMoves)
            {
                //for each combo compute shortest path and points
                var shortestPath = GameRouteGraph.FindAllShortestPathsBetweenDestinationCards(combination.ChosenDestinationCards, Color, numberOfPlayers);
                double lowestCost = int.MaxValue;
                int pointsForShortestRoute = shortestPath.Sum(r => r.PointValue);

                if (shortestPath.Count() == 0)
                {
                    int cost = 1000;
                    lowestCost = cost;
                    pathsPerCombination.Add((combination, new List<GameBoard.Route>(), cost, 0));
                }
                else
                {
                    var cost = CalculateCombinedPathCost(shortestPath, playerTrainCards);
                    
                    //points = (points from destination cards + points from shortest path) * 0.5
                    double points = (combination.ChosenDestinationCards.Sum(c => c.PointValue) + shortestPath.Sum(r => r.PointValue)) * 0.5;
                    cost = cost - points;
                    pathsPerCombination.Add((combination, shortestPath, points, pointsForShortestRoute));
                }
            }

            bestChoice = (pathsPerCombination.First().ticketCombination, pathsPerCombination.First().lowestCostPath, pathsPerCombination.First().lowestCost);

            double lowestPathCost = int.MaxValue;
            foreach (var combination in pathsPerCombination)
            {
                if (combination.lowestCost < lowestPathCost)
                {
                    lowestPathCost = combination.lowestCost;
                    bestChoice = (combination.ticketCombination, combination.lowestCostPath, combination.lowestCost);
                }
            }

            totalPathPoints += bestChoice.lowestCostPath.Sum(r => r.PointValue);

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
