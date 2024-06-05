using TicketToRide.Model.Cards;

namespace TicketToRide.Controllers.GameLog
{
    /// <summary>
    /// Keeps track of the state of train cards after each move (face down / up deck, discard pile)
    /// </summary>
    public class TrainCardsState
    {
        public List<TrainCard> Deck {  get; set; }
        public List<TrainCard> FaceUpDeck {  get; set; }
        public List<TrainCard> DiscardPile {  get; set; }

        public TrainCardsState()
        {

        }

        public TrainCardsState(List<TrainCard> deck, List<TrainCard> faceUpDeck, List<TrainCard> discardPile)
        {
            Deck = MakeListCopy(deck);
            FaceUpDeck = MakeListCopy(faceUpDeck);
            DiscardPile = MakeListCopy(discardPile);
        }

        private List<TrainCard> MakeListCopy(List<TrainCard> list)
        {
            var newList = new List<TrainCard>();

            foreach(var card in list)
            {
                newList.Add(new TrainCard(card));
            }

            return newList;
        }
    }
}
