using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;

namespace TicketToRide.Model.GameBoard
{
    public class Game
    {
        public Board Board { get; set; }

        public IList<Player> Players { get; set; } = new List<Player>();

        public int PlayerTurn { get; set; } = 0;

        public Game(Board board, IList<Player> players)
        {
            this.Board = board;
            this.Players = players;
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

        public ValidateActionMessage ValidateClaimRouteAction(
            int playerIndex, 
            City originCity, 
            City destinationCity,
            TrainColor trainColor,
            int length)
        {
            if (playerIndex != PlayerTurn)
            {
                return new ValidateActionMessage
                {
                    IsValid = false,
                    Message = "It is not this player's turn"
                };
            }


            var isValid = Board.Routes.IsRouteValid(originCity, destinationCity, trainColor, length);

            if (!isValid)
            {
                return new ValidateActionMessage
                {
                    IsValid = false,
                    Message = "Route parameters are invalid"
                };
            }

            var canClaimRoute = Board.Routes.CanClaimRoute(originCity, destinationCity, trainColor, length, Players.ElementAt(playerIndex));

            if (!canClaimRoute)
            {
                return new ValidateActionMessage
                {
                    IsValid = false,
                    Message = "Player does not have necessary train cards"
                };
            }

            return new ValidateActionMessage
            {
                IsValid = true,
                Message = "Player can claim route"
            };
        }

        #region private
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
