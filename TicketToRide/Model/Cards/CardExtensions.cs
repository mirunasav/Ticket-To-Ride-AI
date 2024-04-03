namespace TicketToRide.Model.Cards
{
    public static class CardExtensions
    {
        public static List<T> Shuffle<T>(this List<T> deck) where T : Card
        {
            // Fisher-Yates shuffle algorithm
            Random rng = new Random();

            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }

            return deck;
        }

        public static List<T> Pop<T>(this List<T> cards, int count) where T : Card
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            if (count <= 0 || count > cards.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            List<T> removedItems = cards.GetRange(0, count);
            cards.RemoveRange(0, count);

            return removedItems;
        }

        public static T Pop<T>(this List<T> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            T removedItem = cards.First();
            cards.RemoveAt(0);

            return removedItem;
        }

    }

}
