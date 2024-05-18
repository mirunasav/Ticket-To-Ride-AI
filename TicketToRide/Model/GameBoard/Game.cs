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

        public Game(Board board, IList<Player> players)
        {
            this.Board = board;
            this.Players = players;
        }

        public Game(Board board, IList<Player> players, GameState gameState, int playerTurn)
        {
            Board = board;
            Players = players;
            GameState = gameState;
            PlayerTurn = playerTurn;
        }


        public ValidateActionMessage ValidateAction(PlayerActions action, int playerIndex)
        {
            if(playerIndex != PlayerTurn)
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
                    break;
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
            GameState = GameState.WaitingForPlayerMove;
            ChangePlayerTurn();
        }

        public Player GetPlayer(int playerIndex)
        {
            if(playerIndex < 0 || playerIndex >= Players.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex));
            }

            return Players.ElementAt(playerIndex);
        }

        public void MarkRouteAsClaimed(Route route, Player player)
        {
            ArgumentNullException.ThrowIfNull(route);

            var foundRoute = Board.Routes.GetRoute(route.Origin, route.Destination);

            ArgumentNullException.ThrowIfNull(foundRoute);

            foundRoute.IsClaimed = true;
            foundRoute.ClaimedBy = player.Color;
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
