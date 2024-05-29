using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;

namespace TicketToRide.Model.GameBoard
{
    public class Game
    {
        public Board Board { get; set; }

        public IList<Player> Players { get; set; } = new List<Player>();

        public int PlayerTurn { get; set; } = 0;

        public GameState GameState { get; set; } = GameState.WaitingForPlayerMove;

        public bool IsFinalTurn { get; set; } = false;

        public int LongestContPathLength { get; set; }

        public int LongestContPathPlayerIndex {  get; set; }

        public GameLog GameLog { get; set; }

        public Game(Board board, IList<Player> players)
        {
            this.Board = board;
            this.Players = players;
            this.GameLog = new GameLog(players.Count);
        }

        //for player POV
        public Game(Board board,
            IList<Player> players, 
            GameState gameState, 
            int playerTurn,
            GameLog gameLog
            )
        {
            Board = board;
            Players = players;
            GameState = gameState;
            PlayerTurn = playerTurn;
            GameLog = gameLog;
        }


        public ValidateActionMessage ValidateAction(PlayerActions action, int playerIndex)
        {
            if (playerIndex != PlayerTurn)
            {
                return new ValidateActionMessage
                {
                    IsValid = false,
                    Message = "It is not this player's turn"
                };
            }

            switch (action)
            {
                case PlayerActions.DrawTrainCard:
                    return ValidateDrawTrainCardAction(playerIndex);
                default:
                    return new ValidateActionMessage
                    {
                        IsValid = false,
                        Message = "Invalid action name"
                    };
            }
        }

        public void UpdateStateNextPlayerTurn()
        {
            // if the last player had his turn and it was the final turn, finish game
            //or if the turn ended because of other reasons
            if (PlayerTurn == Players.Count - 1 && IsFinalTurn)
            {
                EndGame();
                return;
            }

            ChangePlayerTurn();
            GameState = GameState.WaitingForPlayerMove;

            UpdateIsFinalTurn();
        }

        public Player GetPlayer(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= Players.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex));
            }

            return Players.ElementAt(playerIndex);
        }

        public void MarkRouteAsClaimed(City origin, City destination, Player player, TrainColor colorUsed)
        {
            var foundRoute = Board.Routes.GetRoute(origin, destination, colorUsed);

            ArgumentNullException.ThrowIfNull(nameof(foundRoute));

                foundRoute.IsClaimed = true;
                foundRoute.ClaimedBy = player.Color;
        }

        public void EndGame()
        {
            if(GameState != GameState.Ended)
            {
                ComputeFinalPoints();
                GameState = GameState.Ended;
            }
        }

        #region private
        private void ChangePlayerTurn()
        {
            if (PlayerTurn == Players.Count - 1)
            {
                PlayerTurn = 0;
                return;
            }

            PlayerTurn += 1;
        }

        private void UpdateIsFinalTurn()
        {
            foreach(var player in Players)
            {
                if(player.RemainingTrains <= 2)
                {
                    IsFinalTurn = true;
                    return;
                }
            }
        }
        private void ComputeFinalPoints()
        {
            int longestContPathLength = 0;
            int longestContPathPlayerIndex = 0;

            foreach (var player in Players)
            {
                //add route points
                foreach (var completedDestination in player.CompletedDestinationCards)
                {
                    player.Points += completedDestination.PointValue;
                }

                foreach (var pendingDestination in player.PendingDestinationCards)
                {
                    player.Points -= pendingDestination.PointValue;
                }

                var longestContPath = player.ClaimedRoutes.LongestContinuousPath();
                if(longestContPath.Item1 > longestContPathLength)
                {
                    longestContPathLength = longestContPath.Item1;
                    longestContPathPlayerIndex = player.PlayerIndex;
                }
            }

            //add longest cont path bonus
            Players[longestContPathPlayerIndex].Points += 10;
            LongestContPathLength = longestContPathLength;
            LongestContPathPlayerIndex = longestContPathPlayerIndex;
        }

        private ValidateActionMessage ValidateDrawTrainCardAction(int playerIndex)
        {
            return new ValidateActionMessage
            {
                IsValid = true,
                Message = "Player can draw train card"
            };
        }
        #endregion
    }

}
