using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public class CardHoarderBot : BotPlayer
    {
        public CardHoarderBot(string name, PlayerColor color, int index) : base(name, color, index)
        {
        }

        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
        {
            if (possibleMoves.ChooseDestinationCardMoves.Count > 0)
            {
                ChooseDestinationCardMove bestMove;

                //choose the one with the least points if player can only choose one
                if (possibleMoves.ChooseDestinationCardMoves.Exists(c => c.ChosenDestinationCards.Count == 1))
                {
                    bestMove = possibleMoves.ChooseDestinationCardMoves.Where(c => c.ChosenDestinationCards.Count == 1)
                   .OrderBy(c => c.ChosenDestinationCards.ElementAt(0).PointValue)
                   .First();

                    return bestMove;
                }

                //select the 2 lowest cost destinations at the beginning of the game
                var allCards = possibleMoves
                   .ChooseDestinationCardMoves
                   .First(c => c.ChosenDestinationCards.Count == 3)
                   .ChosenDestinationCards;

                var leastPointCards = allCards.OrderBy(c => c.PointValue).Take(2);

                bestMove = possibleMoves.ChooseDestinationCardMoves
                    .Where(c => c.ChosenDestinationCards.Count == 2 &&
                    c.ChosenDestinationCards.Contains(leastPointCards.ElementAt(0))
                    && c.ChosenDestinationCards.Contains(leastPointCards.ElementAt(1)))
                    .First();

                return bestMove;
            }

            //first 20 turns: draw cards
            if (game.GameTurn <= 21 && possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                var move = GetDrawTrainCardMove(game.GameTurn, possibleMoves);
                if (move != null)
                {
                    return move;
                }
            }

            //claim routes
            if (possibleMoves.ClaimRouteMoves.Count > 0)
            {
                return GetClaimRouteMove(possibleMoves);
            }
            else if (possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                var move = GetDrawTrainCardMove(game.GameTurn, possibleMoves);
                if (move != null)
                {
                    return move;
                }
            }
            return possibleMoves.DrawDestinationCardMove;
        }

        private Move? GetDrawTrainCardMove(int gameTurn, PossibleMoves possibleMoves)
        {
            //for the first 9 turns, draw cards from the hidden card deck
            if (gameTurn <= 9)
            {
                return possibleMoves.DrawTrainCardMoves.Where(d => d.faceUpCardIndex == -1).First();
            }

            //for the next rounds, draw from face up deck
            //draw the card if the player does not have at least 5 cards from that color
            //no such color? draw from face down deck
            if (possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                var playerCardsGroupedByColor = GetGroupedTrainColors();
                foreach (var drawTrainCardMove in possibleMoves.DrawTrainCardMoves)
                {
                    if ((playerCardsGroupedByColor.ContainsKey(drawTrainCardMove.CardColor) &&
                        playerCardsGroupedByColor[drawTrainCardMove.CardColor] < 5)
                        || (!playerCardsGroupedByColor.ContainsKey(drawTrainCardMove.CardColor)))
                    {
                        return drawTrainCardMove;
                    }
                }
               
                var hiddenDeckMove = possibleMoves.DrawTrainCardMoves.Where(d => d.faceUpCardIndex == -1).FirstOrDefault();
               
                if (hiddenDeckMove is null)
                {
                    //draw a random face up card
                    return possibleMoves.DrawTrainCardMoves.First();
                }
                else
                {
                    return hiddenDeckMove;
                }
            }

            return null;
        }

        private Move GetClaimRouteMove(PossibleMoves possibleMoves)
        {
            var longestRouteMove = possibleMoves.ClaimRouteMoves
                .OrderByDescending(m => m.Route.ElementAt(0).Length)
                .First();

            return longestRouteMove;
        }
    }
}
