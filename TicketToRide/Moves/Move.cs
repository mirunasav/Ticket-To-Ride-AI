using TicketToRide.Controllers.Responses;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public abstract class Move
    {
        public Game Game { get; set; }

        public int PlayerIndex { get; set; }

        public Move(Game game, int playerIndex)
        {
            this.Game = game;
            this.PlayerIndex = playerIndex;
        }

        public abstract MakeMoveResponse Execute();

    }
}
