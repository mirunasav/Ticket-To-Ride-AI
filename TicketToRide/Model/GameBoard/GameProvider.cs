using System.Text.Json;
using TicketToRide.Controllers;
using TicketToRide.Controllers.GameLog;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;
namespace TicketToRide.Model.GameBoard
{
    public class GameProvider
    {
        private Game game;

        private ReloadableGame reloadableGame;

        public Game InitializeGame(int numberOfPlayers, List<PlayerType> playerTypes)
        {
            var board = new Board();
            var players = InitPlayers(numberOfPlayers, playerTypes, board);
            DealCards(board, players);
            var gameLog = CreateGameLog(numberOfPlayers, players.Count(p => p.IsBot));
            game = new Game(board, players, gameLog);

            //write game
            WriteInitialGameStateToFile(game);

            return game;
        }

        public Game GetGame()
        {
            return game;
        }

        public ReloadableGame GetReloadableGame ()
        {
            return reloadableGame;
        }

        public void DeleteGame()
        {
            game = null;
            reloadableGame = null;
        }

        public void SetGame(Game game)
        {
            this.game = game;

            foreach(var route in game.Board.Routes.Routes)
            {
                game.Board.Routes.AddRoute(route);
            }
        }

        public void SetReloadableGame(ReloadableGame reloadableGame)
        {
            this.reloadableGame = reloadableGame;
        }
        public int GetNumberOfPlayers()
        {
            return game.Players.Count;
        }


        #region private
        private List<Player> InitPlayers(int numberOfPlayers, List<PlayerType> playerTypes, Board board)
        {
            var playerList = new List<Player>();
            var playerColors = new List<PlayerColor>();

            InitPlayerColors(numberOfPlayers, playerColors);
            for (int i = 0; i < numberOfPlayers; i++)
            {

                playerList.Add(InitPlayer(i, playerTypes[i], playerColors, board));
            }
            return playerList;
        }

        private Player InitPlayer(int index, PlayerType playerType, IList<PlayerColor> playerColors, Board board)
        {
            var playerColor = GetRandomColor(playerColors);
            switch (playerType)
            {
                case PlayerType.Human:
                    return new Player($"player{index + 1}", playerColor, index);
                case PlayerType.RandomDecisionBot:
                    return new RandomDecisionBot($"player{index + 1}", playerColor, index);
                case PlayerType.PseudoRandomBot:
                    return new PseudoRandomBot($"player{index + 1}", playerColor, index);
                case PlayerType.SimpleStrategyBot:
                    return new SimpleStrategyBot($"player{index + 1}", playerColor, index, board.RouteGraph);
                case PlayerType.LongestRouteBot:
                    return new LongestRouteBot($"player{index + 1}", playerColor, index, board.RouteGraph);
                case PlayerType.EvaluationBasedBot:
                    return new EvaluationBasedBot($"player{index + 1}", playerColor, index, board.RouteGraph);
                case PlayerType.CardHoarderBot:
                    return new CardHoarderBot($"player{index + 1}", playerColor, index);
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

        private GameLog CreateGameLog(int numberOfPlayers, int numberOfBotPlayers)
        {
            (string gameLogEntireFileName,
                string initialGameStateEntireFileName,
                string trainCardsDeckStatesEntireFileName) =
               GenerateRandomFileNames(
               GameConstants.GameLogDirectoryPath,
               GameConstants.InitialGameStatesDirectoryPath,
               GameConstants.TrainCardsStatesDirectoryPath);

            if (numberOfPlayers == numberOfBotPlayers)
            {
                //in a bot only game, log the entire game
                return new BotGameLog(numberOfPlayers, gameLogEntireFileName, initialGameStateEntireFileName, trainCardsDeckStatesEntireFileName);
            }

            return new GameLog(numberOfPlayers, gameLogEntireFileName, initialGameStateEntireFileName, trainCardsDeckStatesEntireFileName);
        }

        private static (string gameLogEntireFileName, string initialGameStateEntireFileName, string trainCardsDeckStatesEntireFileName)
            GenerateRandomFileNames(string gameLogDirectoryPath, string initialGameStateDirectoryPath, string trainCardsDirectoryPath)
        {
            // Ensure the directory exists
            if (!Directory.Exists(gameLogDirectoryPath))
            {
                Directory.CreateDirectory(gameLogDirectoryPath);
            }

            if (!Directory.Exists(initialGameStateDirectoryPath))
            {
                Directory.CreateDirectory(initialGameStateDirectoryPath);
            }

            if (!Directory.Exists(trainCardsDirectoryPath))
            {
                Directory.CreateDirectory(trainCardsDirectoryPath);
            }


            var guid = Guid.NewGuid();
            string gameLogFileName = $"GameLog{guid}.txt";
            string initialGameStateFileName = $"GameState{guid}.json";
            string trainCardsDeckFileName = $"TrainCardsStates{guid}.json";

            string gameLogEntireFileName = Path.Combine(gameLogDirectoryPath, gameLogFileName);
            string initialGameStateEntireFileName = Path.Combine(initialGameStateDirectoryPath, initialGameStateFileName);
            string trainCardsDeckStatesEntireFileName = Path.Combine(trainCardsDirectoryPath, trainCardsDeckFileName);
            return (gameLogEntireFileName, initialGameStateEntireFileName, trainCardsDeckStatesEntireFileName);
        }

        private static void WriteInitialGameStateToFile(Game game)
        {
            var filePath = game.GameLog.InitialGameStateFileName;


            string jsonString = JsonSerializer.Serialize(game);

            // Write the JSON string to the specified file
            File.WriteAllText(filePath, jsonString);
        }

        private static void DealCards(Board board, IList<Player> players)
        {
            DealTrainCards(board, players);
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

        #endregion
    }
}
