using TicketToRide.Moves;

namespace TicketToRide.GameLogs
{
    public class BotGameLog : GameLog
    {
        public BotGameLog(int nunmberOfPlayers, string fileName, string initialGameStateFileName, string trainCardDeckFileName) :
            base(nunmberOfPlayers, fileName, initialGameStateFileName, trainCardDeckFileName)
        {
        }

        public override void LogMove(string playerName, DrawDestinationCardMove drawDestinationCardMove, bool writeToFile = true)
        {
            var shortMessage = $"{playerName} has drawn destination cards.";
            WriteMoveToLog(shortMessage, shortMessage, writeToFile);
        }

        public override void LogMove(string playerName, ChooseDestinationCardMove chooseDestinationCardMove, bool writeToFile = true)
        {
            var shortMessage = $"{playerName} has chosen destination cards.";
            var longMessage = $"{playerName} has chosen the destination cards ";

            foreach (var card in chooseDestinationCardMove.ChosenDestinationCards)
            {
                longMessage += $"{card.Origin}-{card.Destination} {card.PointValue}p ";
            }

            longMessage += "and refused the cards ";

            foreach (var card in chooseDestinationCardMove.NotChosenDestinationCards)
            {
                longMessage += $"{card.Origin}-{card.Destination} {card.PointValue}p ";
            }

            longMessage += ".";

            WriteMoveToLog(shortMessage, longMessage, writeToFile);
        }

        public override void LogMove(string playerName, DrawTrainCardMove drawTrainCardMove, bool writeToFile = true)
        {
            string? shortMessage;
            string? longMessage;
            //if from face up deck
            if (drawTrainCardMove.faceUpCardIndex != -1)
            {
                //message = $"{playerName} has drawn a {drawTrainCardMove.CardColor} card from index {drawTrainCardMove.faceUpCardIndex} face up deck.";
                shortMessage = $"{playerName} has drawn a {drawTrainCardMove.CardColor} card from the face up deck.";
                longMessage = $"{playerName} has drawn a {drawTrainCardMove.CardColor} card from the face up deck from index {drawTrainCardMove.faceUpCardIndex}.";
            }
            else
            {
                shortMessage = $"{playerName} has drawn a card from the face down deck.";
                longMessage = shortMessage;
            }

            WriteMoveToLog(shortMessage, longMessage, writeToFile);
        }

        public override void LogMove(string playerName, ClaimRouteMove claimRouteMove, bool writeToFile = true)
        {
            var message = $"{playerName} has claimed a route from {claimRouteMove.Origin} to {claimRouteMove.Destination} using {claimRouteMove.ColorUsed}.";

            WriteMoveToLog(message, message, writeToFile);
        }

        private void WriteMoveToLog(string shortMessage, string completeMessage, bool writeToFile = true)
        {
            var gameLogLine = new GameLogLine();

            if (GameLogLines.Count >= maxLogLineCount)
            {
                // Remove the first log line to keep the log size within the limit
                GameLogLines.RemoveAt(0);
            }

            gameLogLine.Index = ++LogLineCount;
            gameLogLine.Message = shortMessage;

            GameLogLines.Add(gameLogLine);
            Console.WriteLine($"[#{gameLogLine.Index}] {shortMessage}");

            if (writeToFile)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(GameLogFileName, true))
                    {
                        writer.WriteLine($"[#{gameLogLine.Index}] {completeMessage}");
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
