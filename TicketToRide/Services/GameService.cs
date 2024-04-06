using System.Diagnostics;
using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Moves;

namespace TicketToRide.Services
{
    public class GameService
    {
        private Game game;

        public Game InitializeGame(int numberOfPlayers)
        {
            game = GameInitializer.InitializeGame(numberOfPlayers);
            return game;
        }

        public Game GetGameInstance()
        {
            return game;
        }

        public MakeMoveResponse DrawTrainCard(int playerIndex, int faceUpCardIndex)
        {
            var drawTrainCardMove = new DrawTrainCard(game, playerIndex, faceUpCardIndex);

            var canMakeMoveMessage = CanMakeMove(drawTrainCardMove);

            if(canMakeMoveMessage != ValidMovesMessages.Ok)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = canMakeMoveMessage
                };
            }

            return MakeMove(drawTrainCardMove);
        }

        private MakeMoveResponse MakeMove(Move move)
        {
            var validateMove = move.ValidateMove();

            if (!validateMove.IsValid)
            {
                return validateMove;
            }

            var response = move.Execute();

            return response;
        }

        private bool IsPlayerIndexValid(int playerIndex)
        {
            return !(game.Players.Count < playerIndex || playerIndex < 0);
        }

        private string CanMakeMove(Move move)
        {
            if (game is null)
            {
                return InvalidMovesMessages.UninitializedGame;
            }

            if (!IsPlayerIndexValid(move.playerIndex))
            {
                return InvalidMovesMessages.InvalidPlayerIndex;
            }

            if (move.playerIndex != game.PlayerTurn)
            {
                return InvalidMovesMessages.NotThisPlayersTurn;
            }

            if (move is DrawTrainCard)
            {
                if (game.GameState != GameState.WaitingForPlayerMove &&
                    game.GameState != GameState.DrawingTrainCards)
                {
                    return InvalidMovesMessages.InvalidActionForCurrentGameState;
                }
            }

            return ValidMovesMessages.Ok;
        }

    }
}
