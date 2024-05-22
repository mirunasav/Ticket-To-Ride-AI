using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Model.Players
{
    public class RandomDecisionBot : BotPlayer
    {
        public RandomDecisionBot(string name, PlayerColor color, int index) : base(name, color, index)
        {
        }
        //ca parametru : o lista de possible moves din care sa aleaga
        public override Move GetNextMove(Game game)
        {
            //get all possible moves
            //choose a random one
            //return it
            return new DrawTrainCardMove(game, this.PlayerIndex, -1); // always get cards from normal deck
        }
    }
}
