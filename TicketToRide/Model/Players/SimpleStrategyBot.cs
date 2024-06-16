using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public class SimpleStrategyBot : BotPlayer
    {
        protected RouteGraph GameRouteGraph { get; set; }

        public SimpleStrategyBot(string name, PlayerColor color, int index, RouteGraph routeGraph) : base(name, color, index)
        {
            GameRouteGraph = routeGraph;
        }

        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
        {
            Random random = new Random();
            int randomIndex;

            if (possibleMoves.ChooseDestinationCardMoves.Count > 0)
            {
                ChooseDestinationCardMove bestMove;

                var allCards = possibleMoves
                    .ChooseDestinationCardMoves
                    .First(c => c.ChosenDestinationCards.Count == 3)
                    .ChosenDestinationCards;

                //choose the one with the biggest points and least needed amount of trains to finish
                if (possibleMoves.ChooseDestinationCardMoves.Exists(c => c.ChosenDestinationCards.Count == 1))
                {
                    var bestCard = GetBestDestinationCard(allCards, game);

                    bestMove = possibleMoves.ChooseDestinationCardMoves
                        .Where(c => c.ChosenDestinationCards.Count == 1
                         && c.ChosenDestinationCards.Contains(bestCard)).First();

                    return bestMove;
                }

                //else, choose the one with the most points and the one with the least points to minimise loss

                var bestPointsCard = allCards.OrderByDescending(c => c.PointValue).First();
                var leastPointsCard = allCards.OrderBy(c => c.PointValue).First();

                var bestMoveQuery = possibleMoves.ChooseDestinationCardMoves
                    .Where(c => c.ChosenDestinationCards.Count == 2 &&
                    c.ChosenDestinationCards.Contains(bestPointsCard));

                bestMoveQuery = bestMoveQuery.Where(c => c.ChosenDestinationCards.Contains(leastPointsCard));

                bestMove = bestMoveQuery.First();

                return bestMove;
            }

            var shortestPathRoutes = GameRouteGraph.FindAllShortestPathsBetweenDestinationCards(PendingDestinationCards, Color, game.Board.Routes, game.Players.Count);

            if (possibleMoves.ClaimRouteMoves.Count > 0)
            {
                //choose routes which are on one of the shortest paths
                //between the cities which need to be connected

                if (!(shortestPathRoutes is null || shortestPathRoutes.Count == 0))
                {
                    //see if there is a possible route which is in these 
                    foreach (var possibleClaimRouteMove in possibleMoves.ClaimRouteMoves)
                    {
                        var equalRoutes = shortestPathRoutes.Where(r =>
                        r.Equals(possibleClaimRouteMove.Route.ElementAt(0))
                        && !r.IsClaimed).ToList();

                        if (equalRoutes.Count != 0)
                        {
                            return possibleClaimRouteMove;
                        }
                    }
                }
                else
                {
                    //the cities are unreachable so there is no goal to work to.
                    //if the turn is less than 30 and the number of players is 2, get more tickets

                    if (game.Players.Count == 2 && game.GameTurn <= 40)
                    {
                        return possibleMoves.DrawDestinationCardMove;
                    }

                    if (game.Players.Count == 3 && game.GameTurn <= 30)
                    {
                        return possibleMoves.DrawDestinationCardMove;
                    }

                    //otherwise, Simply claim a route, ordered by length, to bring most points
                    //so as not to risk getting a card and not finishing it

                    var claimRouteMove = possibleMoves.ClaimRouteMoves
                        .OrderByDescending(r => r.Route.ElementAt(0).Length)
                        .First();
                    return claimRouteMove;
                }
            }

            //if there are no routes which can be claimed and are helpful, try to draw train cards
            if (possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                //if there are paths that are useful to be claimed, figure out which train cards should
                //be drawn in order to help
                if (!(shortestPathRoutes is null || shortestPathRoutes.Count == 0))
                {
                    var neededTrainCards = GameRouteGraph.GetLeastTrainColorsNeededForShorthestPaths(shortestPathRoutes);
                    var necessaryFaceUpCardExists = false;

                    //check if a card of the needed color exists
                    foreach (var drawTrainCardMove in possibleMoves.DrawTrainCardMoves)
                    {
                        if ((drawTrainCardMove.CardColor != default) &&
                            (neededTrainCards.ContainsKey(drawTrainCardMove.CardColor)
                            || drawTrainCardMove.CardColor == TrainColor.Locomotive
                            || neededTrainCards.ContainsKey(TrainColor.Grey)))
                        {
                            necessaryFaceUpCardExists = true;
                            break;
                        }
                    }

                    if (!necessaryFaceUpCardExists)
                    {
                        //get face down deck card if the deck is not exhausted
                        var faceDownDeckMove = possibleMoves.DrawTrainCardMoves.FirstOrDefault(move => move.faceUpCardIndex == -1);

                        if (faceDownDeckMove != null)
                        {
                            return faceDownDeckMove;
                        }

                        //otherwise get random card from face up deck
                        randomIndex = random.Next(0, possibleMoves.DrawTrainCardMoves.Count);
                        return possibleMoves.DrawTrainCardMoves[randomIndex];
                    }

                    //if there are useful cards which can be drawn, figure out the most useful one
                    var (neededColorsDictionary, mostFreqColor) = GetRemainingTrainColorsNeededForShorthestPaths(neededTrainCards);

                    var availableTrainColors = new List<TrainColor>();
                    DrawTrainCardMove bestMove = possibleMoves.DrawTrainCardMoves.ElementAt(0);
                    int mostNeededCardCount = int.MaxValue;

                    foreach (var drawTrainCardMove in possibleMoves.DrawTrainCardMoves)
                    {
                        if (mostFreqColor != default && neededColorsDictionary.ContainsKey(TrainColor.Grey))
                        {
                            if (mostFreqColor == drawTrainCardMove.CardColor && mostNeededCardCount < neededColorsDictionary[TrainColor.Grey])
                            {
                                mostNeededCardCount = neededColorsDictionary[TrainColor.Grey];
                                bestMove = drawTrainCardMove;
                            }
                        }

                        if (neededColorsDictionary.ContainsKey(drawTrainCardMove.CardColor))
                        {
                            if (neededColorsDictionary[drawTrainCardMove.CardColor] < mostNeededCardCount)
                            {
                                mostNeededCardCount = neededColorsDictionary[drawTrainCardMove.CardColor];
                                bestMove = drawTrainCardMove;
                            }
                        }
                    }

                    return bestMove;
                }

                //otherwise, draw a random card
                randomIndex = random.Next(0, possibleMoves.DrawTrainCardMoves.Count);
                return possibleMoves.DrawTrainCardMoves[randomIndex];
            }

            //no useful draw train card moves or useful routes to be claimed so just claim randomly
            if (possibleMoves.DrawTrainCardMoves.Count == 0 && possibleMoves.ClaimRouteMoves.Count > 0)
            {
                var claimRouteMove = possibleMoves.ClaimRouteMoves
                    .OrderByDescending(r => r.Route.ElementAt(0).Length)
                    .First();
                return claimRouteMove;
            }

            //if there are not possible draw train card moves
            return possibleMoves.DrawDestinationCardMove;
        }

        protected DestinationCard GetBestDestinationCard(List<DestinationCard> destinationCards, Game game)
        {
            DestinationCard bestCard = null;
            var bestScore = 0.00;

            //foreach card calculate a score : points / needed trains to finish
            foreach (var card in destinationCards)
            {
                var shortestPathRoutes = GameRouteGraph.FindAllShortestPathsBetweenDestinationCards(new List<DestinationCard> { card }, Color, game.Board.Routes, game.Players.Count);

                var routesStillNeeded = shortestPathRoutes.Where(r => !ClaimedRoutes.ContainsRoute(r)).ToList();

                var score = ((double)card.PointValue / routesStillNeeded.Sum(r => r.Length));

                if (score > bestScore)
                {
                    bestCard = card;
                    bestScore = score;
                }

            }

            return bestCard;
        }
        protected (Dictionary<TrainColor, int> neededColorsDictionary, TrainColor mostFreqColor) GetRemainingTrainColorsNeededForShorthestPaths(Dictionary<TrainColor, int> neededCardColors)
        {
            // Initialize a dictionary to track the count of each color in the player's hand
            Dictionary<TrainColor, int> colorCounts = [];
            (TrainColor mostFreqColor, int frequency) = (default, 0);

            foreach (var trainCard in Hand)
            {
                //if the key is contained, decrease by one
                if (neededCardColors.ContainsKey(trainCard.Color))
                {
                    if (neededCardColors[trainCard.Color] > 0)
                    {
                        neededCardColors[trainCard.Color] -= 1;
                    }
                }

                //if the card is locomotive, decrease all counts by one
                if (trainCard.Color == TrainColor.Locomotive)
                {
                    foreach (var color in neededCardColors.Keys.ToList())
                    {
                        if (neededCardColors[color] > 0)
                        {
                            neededCardColors[color] -= 1;
                        }
                    }
                }

                if (colorCounts.ContainsKey(trainCard.Color))
                {
                    colorCounts[trainCard.Color] += 1;
                }
                else
                {
                    colorCounts[trainCard.Color] = 1;
                    if (colorCounts[trainCard.Color] > frequency)
                    {
                        frequency = colorCounts[trainCard.Color];
                        mostFreqColor = trainCard.Color;
                    }
                }
            }

            //get the most frequent color
            if (neededCardColors.ContainsKey(TrainColor.Grey) && mostFreqColor != default)
            {
                if (neededCardColors[TrainColor.Grey] <= frequency)
                {
                    neededCardColors[TrainColor.Grey] = 0;
                }
                else
                {
                    neededCardColors[TrainColor.Grey] -= frequency;
                }
            }

            var orderedColorCounts = neededCardColors.OrderBy(kv => kv.Value);

            // Return the ordered dictionary
            return (orderedColorCounts.ToDictionary(kv => kv.Key, kv => kv.Value), mostFreqColor);
        }
    }
}
