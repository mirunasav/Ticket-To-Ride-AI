using TicketToRide.Controllers.Requests;
using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Model.Players;
using TicketToRide.Moves;


namespace TicketToRide.Services
{
    public class GameService
    {
        private readonly GameProvider gameProvider;

        private readonly MoveValidatorService moveValidatorService;

        private Game game;

        public GameService(
            GameProvider gameProvider,
            MoveValidatorService moveValidatorService)
        {
            this.gameProvider = gameProvider;
            this.moveValidatorService = moveValidatorService;
        }

        public Game InitializeGame(int numberOfPlayers)
        {
            game = gameProvider.InitializeGame(numberOfPlayers);
            return game;
        }

        public Game GetGameInstance(int playerIndex)
        {
            var game = gameProvider.GetGame();

            //check if playerIndex is out of bounds
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(playerIndex, game.Players.Count);

            //hide statistics
            var hiddenStatisticsGame = GetPlayerPOV(playerIndex, game);

            return hiddenStatisticsGame;
        }

        public Game GetGameInstance()
        {
            return gameProvider.GetGame();
        }

        public void DeleteGame()
        {
            gameProvider.DeleteGame();
        }

        public IList<Player> GetPlayers()
        {
            return game.Players;
        }

        public MakeMoveResponse DrawTrainCard(int playerIndex, int faceUpCardIndex)
        {
            var drawTrainCardMove = new DrawTrainCardMove(game, playerIndex, faceUpCardIndex);

            var canMakeMoveMessage = CanMakeMove(drawTrainCardMove);

            if (canMakeMoveMessage != ValidMovesMessages.Ok)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = canMakeMoveMessage
                };
            }

            var validateMove = moveValidatorService.ValidateDrawTrainCardMove(drawTrainCardMove);
            if (!validateMove.IsValid)
            {
                return validateMove;
            }

            var response = drawTrainCardMove.Execute();

            return response;
        }

        public MakeMoveResponse CanClaimRoute(CanClaimRouteRequest request)
        {
            var canClaimRouteMove = new CanClaimRouteMove(game, request.PlayerIndex, request.Origin, request.Destination);

            var canMakeMoveMessage = CanMakeMove(canClaimRouteMove);

            if (canMakeMoveMessage != ValidMovesMessages.Ok)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = canMakeMoveMessage
                };
            }

            var validateMove = moveValidatorService.CanRouteBeClaimed(canClaimRouteMove);
            if (!validateMove.IsValid)
            {
                return validateMove;
            }

            var response = canClaimRouteMove.Execute();
            return response;
        }

        public MakeMoveResponse ClaimRoute(int playerIndex, City origin, City destination, TrainColor colorUsed)
        {
            var claimRouteMove = new ClaimRouteMove(game, playerIndex, colorUsed, origin, destination);

            var canMakeMoveMessage = CanMakeMove(claimRouteMove);

            if (canMakeMoveMessage != ValidMovesMessages.Ok)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = canMakeMoveMessage
                };
            }

            var validateMove = moveValidatorService.ValidateClaimRouteMove(claimRouteMove);

            if (!validateMove.IsValid)
            {
                return validateMove;
            }

            var response = claimRouteMove.Execute();
            return response;
        }

        public MakeMoveResponse DrawDestinationCards(int playerIndex)
        {
            var drawDestinationCardMove = new DrawDestinationCardMove(game, playerIndex);

            var canMakeMoveMessage = CanMakeMove(drawDestinationCardMove);

            if (canMakeMoveMessage != ValidMovesMessages.Ok)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = canMakeMoveMessage
                };
            }

            var validateMove = moveValidatorService.ValidateDrawDestinationCardsMove(drawDestinationCardMove);
            if (!validateMove.IsValid)
            {
                return validateMove;
            }

            var response = drawDestinationCardMove.Execute();
            return response;
        }

        public MakeMoveResponse ChooseDestinationCards(DrawDestinationCardsRequest drawDestinationCardsRequest)
        {
            var chosenDestinations = new List<DestinationCard>();
            var notChosenDestinations = new List<DestinationCard>();

            foreach (var destination in drawDestinationCardsRequest.DestinationCards)
            {
                bool originExists = Enum.TryParse(destination.Origin, out City originCity);
                bool destinationExists = Enum.TryParse(destination.Destination, out City destinationCity);

                if (!originExists || !destinationExists)
                {
                    return new MakeMoveResponse
                    {
                        IsValid = false,
                        Message = InvalidMovesMessages.InvalidCityNumber
                    };
                }

                var destinationOption = GetDestinationCard(originCity, destinationCity);

                if (destinationOption is null)
                {
                    return new MakeMoveResponse
                    {
                        IsValid = false,
                        Message = InvalidMovesMessages.DestinationDoesNotExist
                    };
                }

                if (destination.IsChosen)
                {
                    chosenDestinations.Add(destinationOption);
                }
                else
                {
                    notChosenDestinations.Add(destinationOption);
                }
            }

            var chooseDestinationCardMove = new ChooseDestinationCardMove(
                game,
                drawDestinationCardsRequest.PlayerIndex,
                chosenDestinations,
                notChosenDestinations);

            var canMakeMoveMessage = CanMakeMove(chooseDestinationCardMove);

            if (canMakeMoveMessage != ValidMovesMessages.Ok)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = canMakeMoveMessage
                };
            }

            var validateMove = moveValidatorService.ValidateChooseDestinationCardsMove(chooseDestinationCardMove);
            if (!validateMove.IsValid)
            {
                return validateMove;
            }

            var response = chooseDestinationCardMove.Execute();
            return response;
        }

        #region frontend
        public MakeMoveResponse CanRouteBeClaimed(string origin, string destination)
        {
            bool originExists = Enum.TryParse(origin, out City originCity);
            bool destinationExists = Enum.TryParse(destination, out City destinationCity);

            if (!originExists || !destinationExists)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteDoesNotExist
                };
            }

            var canClaimRouteMove = new CanClaimRouteMove(game, 0, originCity, destinationCity);

            var validateMove = moveValidatorService.CanRouteBeClaimed(canClaimRouteMove);
            return validateMove;
        }
        #endregion
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

            if (move is DrawTrainCardMove)
            {
                if (game.GameState != GameState.WaitingForPlayerMove &&
                    game.GameState != GameState.DecidingAction &&
                    game.GameState != GameState.DrawingTrainCards)
                {
                    return InvalidMovesMessages.InvalidActionForCurrentGameState;
                }
            }

            if (move is CanClaimRouteMove)
            {
                if (game.GameState != GameState.WaitingForPlayerMove &&
                    game.GameState != GameState.DecidingAction)
                {
                    return InvalidMovesMessages.InvalidActionForCurrentGameState;
                }
            }

            if (move is ClaimRouteMove || move is DrawDestinationCardMove)
            {
                if (game.GameState != GameState.WaitingForPlayerMove &&
                    game.GameState != GameState.DecidingAction)
                {
                    return InvalidMovesMessages.InvalidActionForCurrentGameState;
                }
            }

            if (move is ChooseDestinationCardMove)
            {
                if (game.GameState != GameState.WaitingForPlayerMove &&
                  game.GameState != GameState.ChoosingDestinationCards)
                {
                    return InvalidMovesMessages.InvalidActionForCurrentGameState;
                }
            }

            return ValidMovesMessages.Ok;
        }

        private Game GetPlayerPOV(int playerIndex, Game game)
        {
            var players = new List<Player>();
            //foreach player that is not our player, get hidden statistics:
            //name, points, remainingTrains, playerColor, number of Destinationcards, number of cards in hand 
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (i == playerIndex)
                {
                    players.Add(game.Players[i]);
                }
                else
                {
                    players.Add(game.Players[i].GetHiddenStatisticsPlayer());
                }
            }

            var newGame = new Game(game.Board, players, game.GameState, game.PlayerTurn);

            return newGame;
        }

        private DestinationCard? GetDestinationCard(City originCity, City destinationCity)
        {
            return game.Board.DestinationCards
                .Where(d => (d.Origin == originCity && d.Destination == destinationCity)
            || (d.Destination == originCity && d.Origin == destinationCity))
                .FirstOrDefault();
        }

    }
}
