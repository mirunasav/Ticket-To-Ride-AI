namespace TicketToRide.Helpers
{
    public class CombinationGenerator
    {
        public static IEnumerable<List<T>> GetCombinations<T>(List<T> list, int minLength, int maxLength)
        {
            int count = list.Count;
            for (int i = 1; i < (1 << count); i++)
            {
                var combination = new List<T>();
                for (int j = 0; j < count; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        combination.Add(list[j]);
                    }
                }
                if (combination.Count >= minLength && combination.Count <= maxLength)
                {
                    yield return combination;
                }
            }
        }
    }
}
