using System.Text.Json;
using TicketToRide.Controllers.GameLog;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Moves;
using TicketToRide.Services;

namespace TicketToRide.Helpers
{
    public class GameLogParser
    {
        public static List<Move> ParseGameLogFile(RouteService routeService, string[] lines, int numberOfPlayers)
        {
            var moves = new List<Move>();
            int firstDestinationMovesLimit = numberOfPlayers * 2;

            foreach (var line in lines)
            {
                var move = CreateMoveFromLine(routeService, line, firstDestinationMovesLimit);
                if (move != null)
                {
                    moves.Add(move);
                }
            }

            return moves;
        }

        public static TrainCardStates ParseTrainCardStatesFile(string fileName)
        {
            string jsonString = System.IO.File.ReadAllText(fileName);

            TrainCardStates trainCardStates = JsonSerializer.Deserialize<TrainCardStates>(jsonString);

            return trainCardStates;
        }

        private static Move CreateMoveFromLine(RouteService routeService, string line, int firstDestinationMovesLimit)
        {
            var parts = line.Split(' ', 3);
            var number = int.Parse(parts[0].Trim('[', ']', '#'));
            var player = parts[1];
            var playerIndex = int.Parse(player.Replace("player", "")) - 1;

            //draw destination cards
            if (line.Contains("has drawn destination cards"))
            {
                return new DrawDestinationCardMove(playerIndex);
            }

            //choose destination cards
            if (line.Contains("has chosen the destination cards"))
            {
                var chosenCardsStart = line.IndexOf("chosen the destination cards ") + 29;
                var refusedCardsStart = line.IndexOf("and refused the cards ") + 22;

                string chosenCardsPart;
                string refusedCardsPart;

                // Check if there are any refused cards
                if (refusedCardsStart == line.Length - 1)
                {
                    // No refused cards, so just parse chosen cards
                    chosenCardsPart = line.Substring(chosenCardsStart, refusedCardsStart - chosenCardsStart - 23);
                    refusedCardsPart = string.Empty;
                }
                else
                {
                    // There are refused cards
                    chosenCardsPart = line.Substring(chosenCardsStart, refusedCardsStart - chosenCardsStart - 23);
                    refusedCardsPart = line.Substring(refusedCardsStart, line.Length - refusedCardsStart - 2);
                }

                var chosenCards = ParseDestinationCards(chosenCardsPart);
                var refusedCards = ParseDestinationCards(refusedCardsPart);

                return new ChooseDestinationCardMove(playerIndex, chosenCards, refusedCards, true);
            }

            //claim route move
            if (line.Contains("has claimed a route"))
            {
                var routePart = line.Substring(line.IndexOf("from") + 5);
                var toIndex = routePart.IndexOf(" to ");
                var usingIndex = routePart.IndexOf(" using ");

                var origin = routePart.Substring(0, toIndex).Trim();
                var destination = routePart.Substring(toIndex + 4, usingIndex - toIndex - 4).Trim();
                var color = routePart.Substring(usingIndex + 7).Trim('.');

                bool colorExists = Enum.TryParse(color, out TrainColor colorUsed);
                bool originExists = Enum.TryParse(origin, out City originCity);
                bool destinationExists = Enum.TryParse(destination, out City destinationCity);

                if (!colorExists || !originExists || !destinationExists)
                {
                    Console.WriteLine("One of the enum values is not correct.");
                }

                var route = routeService.GetRoute(originCity, destinationCity);

                return new ClaimRouteMove(playerIndex, colorUsed, route);
            }

            //draw train card from hidden deck
            if (line.Contains("has drawn a card from the face down deck"))
            {
                return new DrawTrainCardMove(playerIndex, -1);
            }

            //draw card from face up deck
            if (line.Contains("has drawn a") && line.Contains("from the face up deck"))
            {
                var cardColor = line.Split(' ')[4];
                var indexPart = line.Split(new[] { "from index " }, StringSplitOptions.None)[1];
                var faceUpCardIndex = int.Parse(indexPart.Trim('.'));

                bool colorExists = Enum.TryParse(cardColor, out TrainColor color);

                return new DrawTrainCardMove(playerIndex, faceUpCardIndex);
            }

            else
            {
                throw new ArgumentException("Game log file does not match format.");
            }
        }

        private static List<DestinationCard> ParseDestinationCards(string cardsPart)
        {
            if(cardsPart == string.Empty)
            {
                return [];
            }

            var cards = new List<DestinationCard>();
            var cardParts = cardsPart.Split(' ');

            for (int i = 0; i < cardParts.Length; i += 2)
            {
                var cities = cardParts[i].Split('-');
                var points = int.Parse(cardParts[i + 1].Replace("p", "").Trim());

                bool originExists = Enum.TryParse(cities[0].Trim(), out City originCity);
                bool destinationExists = Enum.TryParse(cities[1].Trim(), out City destinationCity);

                if (!originExists || !destinationExists)
                {
                    Console.WriteLine("One of the enum values is not correct.");
                }

                cards.Add(new DestinationCard
                {
                    Origin = originCity,
                    Destination = destinationCity,
                    PointValue = points
                });
            }

            return cards;

        }
    }
}
