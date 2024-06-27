using System.Text.Json;
using TicketToRide.Model.Constants;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.GameLogs
{
    public class GameLog
    {
        public string GameLogFileName { get; set; }

        public string InitialGameStateFileName { get; set; }
        public string TrainCardsFileName { get; set; }

        public int maxLogLineCount { get; set; }

        public int LogLineCount { get; set; } = 0;

        public IList<GameLogLine> GameLogLines { get; set; } = new List<GameLogLine>();

        private TrainCardStates trainCardsStates { get; set; }

        public GameLog() { }

        public GameLog(int numberOfPlayers, string fileName, string initialGameStateFileName, string trainCardsFileName)
        {
            if (numberOfPlayers == 2)
            {
                maxLogLineCount = GameConstants.MaxNumberOfLogLines2PlayerGame;
            }
            else if (numberOfPlayers == 3)
            {
                maxLogLineCount = GameConstants.MaxNumberOfLogLines3PlayerGame;
            }
            else
            {
                maxLogLineCount = GameConstants.MaxNumberOfLogLines4PlayerGame;
            }

            GameLogFileName = fileName;
            InitialGameStateFileName = initialGameStateFileName;
            TrainCardsFileName = trainCardsFileName;
            trainCardsStates = new TrainCardStates();
        }

        public void LogMove(Move move, string playerName, bool writeToFile = true)
        {
            if (move is DrawDestinationCardMove drawDestinationCardMove)
            {
                LogMove(playerName, drawDestinationCardMove, writeToFile);
                return;
            }

            if (move is ChooseDestinationCardMove chooseDestinationCardMove)
            {
                LogMove(playerName, chooseDestinationCardMove, writeToFile);
                return;
            }

            if (move is DrawTrainCardMove drawTrainCardMove)
            {
                LogMove(playerName, drawTrainCardMove, writeToFile);
                return;
            }

            if (move is ClaimRouteMove claimRouteMove)
            {
                LogMove(playerName, claimRouteMove, writeToFile);
                return;
            }
        }

        public void UpdateTrainCardsDeckStates(Game game)
        {
            var newState = new TrainCardsState(game.Board.Deck, game.Board.FaceUpDeck, game.Board.DiscardPile);
            trainCardsStates.CardStates.Add(newState);
        }

        public void LogTrainCardsDeckStates()
        {
            string jsonString = JsonSerializer.Serialize(trainCardsStates);

            // Write the JSON string to the specified file
            File.WriteAllText(TrainCardsFileName, jsonString);
        }

        public virtual void LogMove(string playerName, DrawDestinationCardMove drawDestinationCardMove, bool writeToFile = true)
        {
            var message = $"{playerName} has drawn destination cards.";

            WriteMoveToLog(message, writeToFile);
        }

        public virtual void LogMove(string playerName, ChooseDestinationCardMove chooseDestinationCardMove, bool writeToFile = true)
        {
            var message = $"{playerName} has chosen destination cards.";

            WriteMoveToLog(message, writeToFile);
        }

        public virtual void LogMove(string playerName, DrawTrainCardMove drawTrainCardMove, bool writeToFile = true)
        {
            string? message;

            //if from face up deck
            if (drawTrainCardMove.faceUpCardIndex != -1)
            {
                //message = $"{playerName} has drawn a {drawTrainCardMove.CardColor} card from index {drawTrainCardMove.faceUpCardIndex} face up deck.";
                message = $"{playerName} has drawn a {drawTrainCardMove.CardColor} card from the face up deck.";
            }
            else
            {
                message = $"{playerName} has drawn a card from the face down deck.";
            }

            WriteMoveToLog(message, writeToFile);
        }

        public virtual void LogMove(string playerName, ClaimRouteMove claimRouteMove, bool writeToFile = true)
        {
            var message = $"{playerName} has claimed a route from {claimRouteMove.Origin} to {claimRouteMove.Destination} using {claimRouteMove.ColorUsed}.";

            WriteMoveToLog(message, writeToFile);
        }

        private void WriteMoveToLog(string gameLogMessage, bool writeToFile = true)
        {
            var gameLogLine = new GameLogLine();

            if (GameLogLines.Count >= maxLogLineCount)
            {
                // Remove the first log line to keep the log size within the limit
                GameLogLines.RemoveAt(0);
            }

            gameLogLine.Index = ++LogLineCount;
            gameLogLine.Message = gameLogMessage;

            GameLogLines.Add(gameLogLine);
            Console.WriteLine($"[#{gameLogLine.Index}] {gameLogMessage}");
            if (writeToFile)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(GameLogFileName, true))
                    {
                        writer.WriteLine($"[#{gameLogLine.Index}] {gameLogMessage}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }

        }

    }
}
