using TicketToRide.Model.Constants;

namespace TicketToRide.Model
{
    public class GameLog
    {
        public readonly int maxLogLineCount;

        public int LogLineCount { get; set; } = 0;
        
        public IList<GameLogLine> GameLogLines { get; set; } = new List<GameLogLine>();

        public GameLog(int numberOfPlayers)
        {
            if(numberOfPlayers == 2)
            {
                maxLogLineCount = GameConstants.MaxNumberOfLogLines2PlayerGame;
            }
            else if(numberOfPlayers == 3)
            {
                maxLogLineCount = GameConstants.MaxNumberOfLogLines3PlayerGame;
            }
            else
            {
                maxLogLineCount = GameConstants.MaxNumberOfLogLines4PlayerGame;
            }
        }
    }
}
