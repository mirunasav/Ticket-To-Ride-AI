using TicketToRide.Controllers.Responses;
using TicketToRide.Model;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public abstract class Move
    {
        public int PlayerIndex { get; set; }

        public Move(int playerIndex)
        {
            this.PlayerIndex = playerIndex;
        }

        public abstract MakeMoveResponse Execute(Game game);
    }
}
