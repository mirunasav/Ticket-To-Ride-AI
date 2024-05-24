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
        public override Move GetNextMove(Game game, PossibleMovesDto possibleMoves)
        {
            //get all possible moves
            //choose a random one
            //return it
            //Random random = new Random();
            //int randomIndex = random.Next(1, 4);

            //switch (randomIndex)
            //{
            //    case 1: // draw train card moves
            //        if(possibleMoves.DrawTrainCardMoves.Count > 0)
            //        {
            //            var randomTrainCardIndex = random.Next(0, possibleMoves.DrawTrainCardMoves.Count);
            //            return possibleMoves.DrawTrainCardMoves[randomTrainCardIndex];

            //        }


            //}
            return new DrawTrainCardMove(game, this.PlayerIndex, -1); // always get cards from normal deck
        }
    }
}
