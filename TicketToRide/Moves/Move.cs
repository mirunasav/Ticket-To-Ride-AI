using TicketToRide.Controllers.Responses;
using TicketToRide.Model;
using TicketToRide.Model.Constants;
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

        public void LogMove(GameLog gameLog, string gameLogMessage)
        {
            var gameLogLine = new GameLogLine();

            if (gameLog.GameLogLines.Count >= gameLog.maxLogLineCount)
            {
                // Remove the first log line to keep the log size within the limit
                gameLog.GameLogLines.RemoveAt(0);
            }

            gameLogLine.Index = ++gameLog.LogLineCount;
            gameLogLine.Message = gameLogMessage;

            gameLog.GameLogLines.Add(gameLogLine);
            Console.WriteLine($"[#{gameLogLine.Index}] {gameLogMessage}");
        }

    }
}
