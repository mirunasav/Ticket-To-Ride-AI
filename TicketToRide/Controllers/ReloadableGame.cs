using TicketToRide.Controllers.GameLog;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Controllers
{
    public class ReloadableGame
    {
        public Game Game {  get; set; }

        public List<Move> MoveSequence { get; set; }

        public TrainCardStates TrainCardsStates { get; set; }

        public ReloadableGame(Game game, List<Move> moves, TrainCardStates states) 
        {
            Game = game;
            Game.IsGameAReplay = true;
            MoveSequence = moves;
            TrainCardsStates = states;
        }
    }
}
