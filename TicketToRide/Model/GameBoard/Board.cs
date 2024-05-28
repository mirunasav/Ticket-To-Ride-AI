using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;

namespace TicketToRide.Model.GameBoard
{
    public class Board
    {
        public BoardRouteCollection Routes { get; set; } = new BoardRouteCollection();

        public List<DestinationCard> DestinationCards { get; set; } = new List<DestinationCard>();

        public List<TrainCard> Deck { get; set; } = new List<TrainCard>();

        public List<TrainCard> FaceUpDeck { get; set; } = new List<TrainCard>();

        public List<TrainCard> DiscardPile { get; set; } = new List<TrainCard>();

        public Board()
        {
            CreateRoutes();
            CreateDestinationCards();
            CreateTrainCardDeck();
            SetupBoard();
        }

        public void SetupBoard()
        {
            Deck = Deck.Shuffle();
            PopulateFaceUpDeck();
        }

        #region private
        private void CreateRoutes()
        {
            Routes.AddRoute(City.Vancouver, City.Calgary, TrainColor.Grey, 3);
            Routes.AddRoute(City.Calgary, City.Winnipeg, TrainColor.White, 6);
            Routes.AddRoute(City.Winnipeg, City.SaultSaintMarie, TrainColor.Grey, 6);
            Routes.AddRoute(City.SaultSaintMarie, City.Montreal, TrainColor.Black, 5);
            Routes.AddRoute(City.Montreal, City.Boston, TrainColor.Grey, 2);
            Routes.AddRoute(City.Montreal, City.Boston, TrainColor.Grey, 2);
            Routes.AddRoute(City.Vancouver, City.Seattle, TrainColor.Grey, 1);
            Routes.AddRoute(City.Vancouver, City.Seattle, TrainColor.Grey, 1);
            Routes.AddRoute(City.Seattle, City.Calgary, TrainColor.Grey, 4);
            Routes.AddRoute(City.Calgary, City.Helena, TrainColor.Grey, 4);
            Routes.AddRoute(City.Helena, City.Winnipeg, TrainColor.Blue, 4);
            Routes.AddRoute(City.Winnipeg, City.Duluth, TrainColor.Black, 4);
            Routes.AddRoute(City.Duluth, City.SaultSaintMarie, TrainColor.Grey, 3);
            Routes.AddRoute(City.SaultSaintMarie, City.Toronto, TrainColor.Grey, 2);
            Routes.AddRoute(City.Toronto, City.Montreal, TrainColor.Grey, 3);
            Routes.AddRoute(City.Portland, City.Seattle, TrainColor.Grey, 1);
            Routes.AddRoute(City.Portland, City.Seattle, TrainColor.Grey, 1);
            Routes.AddRoute(City.Seattle, City.Helena, TrainColor.Yellow, 6);
            Routes.AddRoute(City.Helena, City.Duluth, TrainColor.Orange, 6);
            Routes.AddRoute(City.Duluth, City.Toronto, TrainColor.Purple, 6);
            Routes.AddRoute(City.Toronto, City.Pittsburgh, TrainColor.Grey, 2);
            Routes.AddRoute(City.Montreal, City.NewYork, TrainColor.Blue, 3);
            Routes.AddRoute(City.NewYork, City.Boston, TrainColor.Yellow, 2);
            Routes.AddRoute(City.NewYork, City.Boston, TrainColor.Red, 2);
            Routes.AddRoute(City.Portland, City.SanFrancisco, TrainColor.Green, 5);
            Routes.AddRoute(City.Portland, City.SanFrancisco, TrainColor.Purple, 5);
            Routes.AddRoute(City.Portland, City.SaltLakeCity, TrainColor.Blue, 5);
            Routes.AddRoute(City.SanFrancisco, City.SaltLakeCity, TrainColor.Orange, 5);
            Routes.AddRoute(City.SanFrancisco, City.SaltLakeCity, TrainColor.White, 5);
            Routes.AddRoute(City.SaltLakeCity, City.Helena, TrainColor.Purple, 3);
            Routes.AddRoute(City.Helena, City.Denver, TrainColor.Green, 4);
            Routes.AddRoute(City.SaltLakeCity, City.Denver, TrainColor.Red, 3);
            Routes.AddRoute(City.SaltLakeCity, City.Denver, TrainColor.Yellow, 3);
            Routes.AddRoute(City.Helena, City.Omaha, TrainColor.Red, 5);
            Routes.AddRoute(City.Omaha, City.Duluth, TrainColor.Grey, 2);
            Routes.AddRoute(City.Omaha, City.Duluth, TrainColor.Grey, 2);
            Routes.AddRoute(City.Duluth, City.Chicago, TrainColor.Red, 3);
            Routes.AddRoute(City.Omaha, City.Chicago, TrainColor.Blue, 4);
            Routes.AddRoute(City.Chicago, City.Toronto, TrainColor.White, 4);
            Routes.AddRoute(City.Chicago, City.Pittsburgh, TrainColor.Orange, 3);
            Routes.AddRoute(City.Chicago, City.Pittsburgh, TrainColor.Black, 3);
            Routes.AddRoute(City.Pittsburgh, City.NewYork, TrainColor.White, 2);
            Routes.AddRoute(City.Pittsburgh, City.NewYork, TrainColor.Green, 2);
            Routes.AddRoute(City.NewYork, City.Washington, TrainColor.Orange, 2);
            Routes.AddRoute(City.NewYork, City.Washington, TrainColor.Black, 2);
            Routes.AddRoute(City.SanFrancisco, City.LosAngeles, TrainColor.Yellow, 3);
            Routes.AddRoute(City.SanFrancisco, City.LosAngeles, TrainColor.Purple, 3);
            Routes.AddRoute(City.LosAngeles, City.LasVegas, TrainColor.Grey, 2);
            Routes.AddRoute(City.LasVegas, City.SaltLakeCity, TrainColor.Orange, 3);
            Routes.AddRoute(City.LosAngeles, City.Phoenix, TrainColor.Grey, 3);
            Routes.AddRoute(City.LosAngeles, City.ElPaso, TrainColor.Black, 6);
            Routes.AddRoute(City.Phoenix, City.Denver, TrainColor.White, 5);
            Routes.AddRoute(City.Phoenix, City.SantaFe, TrainColor.Grey, 3);
            Routes.AddRoute(City.Phoenix, City.ElPaso, TrainColor.Grey, 3);
            Routes.AddRoute(City.ElPaso, City.SantaFe, TrainColor.Grey, 2);
            Routes.AddRoute(City.SantaFe, City.Denver, TrainColor.Grey, 2);
            Routes.AddRoute(City.Denver, City.KansasCity, TrainColor.Black, 4);
            Routes.AddRoute(City.Denver, City.KansasCity, TrainColor.Orange, 4);
            Routes.AddRoute(City.Omaha, City.Duluth, TrainColor.Grey, 2);
            Routes.AddRoute(City.Omaha, City.Duluth, TrainColor.Grey, 2);
            Routes.AddRoute(City.Omaha, City.KansasCity, TrainColor.Grey, 1);
            Routes.AddRoute(City.Omaha, City.KansasCity, TrainColor.Grey, 1);
            Routes.AddRoute(City.Denver, City.OklahomaCity, TrainColor.Red, 4);
            Routes.AddRoute(City.SantaFe, City.OklahomaCity, TrainColor.Blue, 2);
            Routes.AddRoute(City.ElPaso, City.OklahomaCity, TrainColor.Yellow, 5);
            Routes.AddRoute(City.ElPaso, City.Dallas, TrainColor.Red, 4);
            Routes.AddRoute(City.ElPaso, City.Houston, TrainColor.Green, 6);
            Routes.AddRoute(City.KansasCity, City.SaintLouis, TrainColor.Blue, 2);
            Routes.AddRoute(City.KansasCity, City.SaintLouis, TrainColor.Purple, 2);
            Routes.AddRoute(City.SaintLouis, City.Chicago, TrainColor.Green, 2);
            Routes.AddRoute(City.SaintLouis, City.Chicago, TrainColor.White, 2);
            Routes.AddRoute(City.SaintLouis, City.Pittsburgh, TrainColor.Green, 5);
            Routes.AddRoute(City.Pittsburgh, City.Washington, TrainColor.Grey, 2);
            Routes.AddRoute(City.SaintLouis, City.Nashville, TrainColor.Grey, 2);
            Routes.AddRoute(City.KansasCity, City.OklahomaCity, TrainColor.Grey, 2);
            Routes.AddRoute(City.KansasCity, City.OklahomaCity, TrainColor.Grey, 2);
            Routes.AddRoute(City.OklahomaCity, City.Dallas, TrainColor.Grey, 2);
            Routes.AddRoute(City.OklahomaCity, City.Dallas, TrainColor.Grey, 2);
            Routes.AddRoute(City.Dallas, City.Houston, TrainColor.Grey, 1);
            Routes.AddRoute(City.Dallas, City.Houston, TrainColor.Grey, 1);
            Routes.AddRoute(City.OklahomaCity, City.LittleRock, TrainColor.Grey, 2);
            Routes.AddRoute(City.LittleRock, City.SaintLouis, TrainColor.Grey, 2);
            Routes.AddRoute(City.SaintLouis, City.Nashville, TrainColor.Grey, 2);
            Routes.AddRoute(City.Nashville, City.Pittsburgh, TrainColor.Yellow, 4);
            Routes.AddRoute(City.Pittsburgh, City.Raleigh, TrainColor.Grey, 2);
            Routes.AddRoute(City.Nashville, City.Raleigh, TrainColor.Black, 3);
            Routes.AddRoute(City.Raleigh, City.Washington, TrainColor.Grey, 2);
            Routes.AddRoute(City.Raleigh, City.Washington, TrainColor.Grey, 2);
            Routes.AddRoute(City.Raleigh, City.Charleston, TrainColor.Grey, 2);
            Routes.AddRoute(City.Dallas, City.LittleRock, TrainColor.Grey, 2);
            Routes.AddRoute(City.LittleRock, City.Nashville, TrainColor.White, 3);
            Routes.AddRoute(City.Nashville, City.Atlanta, TrainColor.Grey, 1);
            Routes.AddRoute(City.Atlanta, City.Raleigh, TrainColor.Grey, 2);
            Routes.AddRoute(City.Atlanta, City.Raleigh, TrainColor.Grey, 2);
            Routes.AddRoute(City.Atlanta, City.Charleston, TrainColor.Grey, 2);
            Routes.AddRoute(City.LittleRock, City.NewOrleans, TrainColor.Green, 3);
            Routes.AddRoute(City.Houston, City.NewOrleans, TrainColor.Grey, 2);
            Routes.AddRoute(City.NewOrleans, City.Atlanta, TrainColor.Yellow, 4);
            Routes.AddRoute(City.NewOrleans, City.Atlanta, TrainColor.Orange, 4);
            Routes.AddRoute(City.NewOrleans, City.Miami, TrainColor.Red, 6);
            Routes.AddRoute(City.Atlanta, City.Miami, TrainColor.Blue, 5);
            Routes.AddRoute(City.Charleston, City.Miami, TrainColor.Purple, 4);
            Routes.AddRoute(City.Omaha, City.Denver, TrainColor.Purple, 4);
        }

        private void CreateDestinationCards()
        {
            DestinationCards.Add(new DestinationCard(City.NewYork, City.Atlanta, 6));
            DestinationCards.Add(new DestinationCard(City.Winnipeg, City.LittleRock, 11));
            DestinationCards.Add(new DestinationCard(City.Boston, City.Miami, 12));
            DestinationCards.Add(new DestinationCard(City.LosAngeles, City.Chicago, 16));
            DestinationCards.Add(new DestinationCard(City.Montreal, City.Atlanta, 9));
            DestinationCards.Add(new DestinationCard(City.Seattle, City.LosAngeles, 9));
            DestinationCards.Add(new DestinationCard(City.KansasCity, City.Houston, 5));
            DestinationCards.Add(new DestinationCard(City.Chicago, City.NewOrleans, 7));
            DestinationCards.Add(new DestinationCard(City.Seattle, City.NewYork, 22));
            DestinationCards.Add(new DestinationCard(City.Portland, City.Nashville, 17));
            DestinationCards.Add(new DestinationCard(City.SaultSaintMarie, City.OklahomaCity, 9));
            DestinationCards.Add(new DestinationCard(City.Vancouver, City.SantaFe, 13));
            DestinationCards.Add(new DestinationCard(City.SanFrancisco, City.Atlanta, 17));
            DestinationCards.Add(new DestinationCard(City.Vancouver, City.Montreal, 20));
            DestinationCards.Add(new DestinationCard(City.Montreal, City.NewOrleans, 13));
            DestinationCards.Add(new DestinationCard(City.LosAngeles, City.NewYork, 21));
            DestinationCards.Add(new DestinationCard(City.Calgary, City.SaltLakeCity, 7));
            DestinationCards.Add(new DestinationCard(City.Denver, City.Pittsburgh, 11));
            DestinationCards.Add(new DestinationCard(City.Helena, City.LosAngeles, 8));
            DestinationCards.Add(new DestinationCard(City.Calgary, City.Phoenix, 13));
            DestinationCards.Add(new DestinationCard(City.Chicago, City.SantaFe, 9));
            DestinationCards.Add(new DestinationCard(City.Toronto, City.Miami, 10)); ;
            DestinationCards.Add(new DestinationCard(City.Dallas, City.NewYork, 11));
            DestinationCards.Add(new DestinationCard(City.Duluth, City.Houston, 8));
            DestinationCards.Add(new DestinationCard(City.SaultSaintMarie, City.Nashville, 8));
            DestinationCards.Add(new DestinationCard(City.Duluth, City.ElPaso, 10));
            DestinationCards.Add(new DestinationCard(City.Winnipeg, City.Houston, 12));
            DestinationCards.Add(new DestinationCard(City.Denver, City.ElPaso, 4));
            DestinationCards.Add(new DestinationCard(City.LosAngeles, City.Miami, 20));
            DestinationCards.Add(new DestinationCard(City.Portland, City.Phoenix, 11));

            DestinationCards = DestinationCards.Shuffle();
        }

        private void CreateTrainCardDeck()
        {
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Red, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Purple, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Black, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Orange, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.White, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Blue, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Green, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Yellow, 12));
            Deck.AddRange(CreateTrainCardColorSet(TrainColor.Locomotive, 14));

            Deck.Shuffle();
        }

        private IEnumerable<TrainCard> CreateTrainCardColorSet(TrainColor color, int count)
        {
            var cards = new List<TrainCard>();

            for (int i = 0; i < count; i++)
            {
                cards.Add(new TrainCard() { Color = color });
            }

            return cards;
        }

        private void RefillFaceUpDeck()
        {
            while (FaceUpDeck.Count < 5)
            {
                if (Deck.Count > 0)
                {
                    var newCard = Deck.Pop(1);
                    if(newCard != null)
                    {
                        FaceUpDeck.AddRange(newCard);
                    }
                }
                else
                {
                    break;
                }

            }
        }

        //The 5 cards that are shown : after drawing a card from the face up deck
        public void PopulateFaceUpDeck()
        {
            //this should only happen when the deck is exhausted
            if (Deck.Count < 5)
            {
                RefillDeck();
            }

            RefillFaceUpDeck();

            var locomotiveCount = FaceUpDeck.Where(c => c.Color == TrainColor.Locomotive).Count();

            while (locomotiveCount >= 3)
            {

                DiscardPile.AddRange(FaceUpDeck);
                FaceUpDeck.Clear();

                if (Deck.Count < 5)
                {
                    RefillDeck();
                }

                RefillFaceUpDeck();
                locomotiveCount = FaceUpDeck.Where(c => c.Color == TrainColor.Locomotive).Count();
            }
        }

        public void ChangeFaceUpLocomotiveStatus(bool isAvailable)
        {
            foreach (var card in FaceUpDeck)
            {
                if (card.Color == TrainColor.Locomotive)
                {
                    card.IsAvailable = isAvailable;
                }
            }
        }

        //when there are no more cards to be drawn, refill the deck
        private void RefillDeck()
        {
            if (DiscardPile.Count > 0)
            {
                var cards = DiscardPile;
                cards.AddRange(Deck);

                cards.Shuffle();

                Deck = cards;
                DiscardPile.Clear();
            }
        }

        #endregion
    }

}
