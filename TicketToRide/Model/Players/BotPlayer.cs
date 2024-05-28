using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public abstract class BotPlayer : Player
    {
        public BotPlayer(string name, PlayerColor color, int index) : base(name, color, index)
        {
            IsBot = true;
        }

        public abstract Move GetNextMove(Game game, PossibleMoves possibleMoves);
    }
}
