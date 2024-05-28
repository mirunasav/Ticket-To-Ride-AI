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

        public override Move GetNextMove(Game game, PossibleMoves possibleMoves)
        {
            //pot lua cu o pondere: adaug destination card move de 1/20 ori ca sa nu fie 1/200
            var allMoves = possibleMoves.GetAllPossibleMoves();

            Random random = new Random();
            int randomIndex = random.Next(0, allMoves.Count);

            return allMoves[randomIndex];
        }
    }
}
