using TicketToRide.Model.Cards;
using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;
namespace TicketToRide.Model.GameBoard
{
    public class GameProvider
    {
        private Game game;

        public Game InitializeGame(int numberOfPlayers, List<PlayerType> playerTypes)
        {
            var players = InitPlayers(numberOfPlayers, playerTypes);
            var board = new Board();
            DealCards(board, players);
            game = new Game(board, players);
            return game;
        }

        public Game GetGame()
        {
            return game;
        }

        public void DeleteGame()
        {
            game = null;
        }

        public int GetNumberOfPlayers()
        {
            return game.Players.Count;
        }


        #region private
        private List<Player> InitPlayers(int numberOfPlayers, List<PlayerType> playerTypes)
        {
            var playerList = new List<Player>();
            var playerColors = new List<PlayerColor>();

            InitPlayerColors(numberOfPlayers, playerColors);
            for (int i = 0; i < numberOfPlayers; i++)
            {

                playerList.Add(InitPlayer(i, playerTypes[i], playerColors));
            }
            return playerList;
        }

        private static Player InitPlayer(int index, PlayerType playerType, IList<PlayerColor> playerColors)
        {
            var playerColor = GetRandomColor(playerColors);
            switch (playerType)
            {
                case PlayerType.Human:
                    return new Player($"player{index + 1}", playerColor, index);
                case PlayerType.RandomDecisionBot:
                    return new RandomDecisionBot($"player{index + 1}", playerColor, index);
                default:
                    return new Player($"player{index + 1}", playerColor, index);
            }
        }

        private static void InitPlayerColors(int numberOfPlayers, IList<PlayerColor> playerColors)
        {
            var colors = Enum.GetValues(typeof(PlayerColor)).Cast<PlayerColor>().ToArray();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                playerColors.Add(colors[i]);
            }
        }

        private static PlayerColor GetRandomColor(IList<PlayerColor> playerColors)
        {
            if (playerColors.Count == 0)
            {
                throw new InvalidOperationException("PlayerColors list is empty");
            }

            Random rand = new Random();
            int randomIndex = rand.Next(0, playerColors.Count);

            PlayerColor selectedColor = playerColors[randomIndex];

            playerColors.RemoveAt(randomIndex);

            return selectedColor;
        }

        private static void DealCards(Board board, IList<Player> players)
        {
            DealTrainCards(board, players);
            DealDestinationCards(board, players);
        }

        private static void DealTrainCards(Board board, IList<Player> players)
        {
            for (int i = 0; i < 4; i++)
            {
                foreach (var player in players)
                {
                    player.Hand.Add(board.Deck.Pop());
                }
            }
        }

        private static void DealDestinationCards(Board board, IList<Player> players)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (var player in players)
                {
                    player.PendingDestinationCards.Add(board.DestinationCards.Pop());
                }
            }
        }
        #endregion
    }
}
