using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public class PseudoRandomBot : BotPlayer
    //favorises longer routes
    {
        public PseudoRandomBot(string name, PlayerColor color, int index) : base(name, color, index)
        {
        }

        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
        {
            Random random = new Random();
            int randomIndex;

            //favorise building longer routes
            if (possibleMoves.ClaimRouteMoves.Count > 0)
            {
                var claimRouteMove = possibleMoves.ClaimRouteMoves
                    .Where(r => r.Route.ElementAt(0).Length > 2)
                    .OrderByDescending(r => r.Route.ElementAt(0).Length)
                    .FirstOrDefault();

                if (claimRouteMove != null)
                {
                    return claimRouteMove;
                }
            }

            //otherwise: draw train card
            if (possibleMoves.DrawTrainCardMoves.Count > 0)
            {
                randomIndex = random.Next(0, possibleMoves.DrawTrainCardMoves.Count);
                return possibleMoves.DrawTrainCardMoves[randomIndex];
            }

            if (possibleMoves.DrawDestinationCardMove != null)
            {
                return possibleMoves.DrawDestinationCardMove;
            }
            else if(possibleMoves.ChooseDestinationCardMoves.Count > 0)
            {
                randomIndex = random.Next(0, possibleMoves.ChooseDestinationCardMoves.Count);
                return possibleMoves.ChooseDestinationCardMoves[randomIndex];
            }

            //otherwise just choose a random claim route move
            randomIndex = random.Next(0, possibleMoves.ClaimRouteMoves.Count);
            return possibleMoves.ClaimRouteMoves[randomIndex];
        }
    }
}
