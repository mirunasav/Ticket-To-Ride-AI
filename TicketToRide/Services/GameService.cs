using System.Collections.Generic;
using TicketToRide.Controllers.Requests;
using TicketToRide.Controllers.Responses;
using TicketToRide.Helpers;
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

        private Object gameLock = new Object();

        public GameService(
            GameProvider gameProvider,
            MoveValidatorService moveValidatorService)
        {
            this.gameProvider = gameProvider;
            this.moveValidatorService = moveValidatorService;
        }

        public Game InitializeGame(int numberOfPlayers, List<PlayerType> playerTypes)
        {
            game = gameProvider.InitializeGame(numberOfPlayers, playerTypes);
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
            var drawTrainCardMove = new DrawTrainCardMove(playerIndex, faceUpCardIndex);

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

            var response = drawTrainCardMove.Execute(game);

            return response;
        }

        public MakeMoveResponse CanClaimRoute(CanClaimRouteRequest request)
        {
            var canClaimRouteMove = new CanClaimRouteMove(request.PlayerIndex, request.Origin, request.Destination);

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

            var response = canClaimRouteMove.Execute(game);
            return response;
        }

        public MakeMoveResponse ClaimRoute(int playerIndex, City origin, City destination, TrainColor colorUsed)
        {
            var claimRouteMove = new ClaimRouteMove(playerIndex, colorUsed, origin, destination);

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

            var response = claimRouteMove.Execute(game);
            return response;
        }

        public MakeMoveResponse DrawDestinationCards(int playerIndex)
        {
            var drawDestinationCardMove = new DrawDestinationCardMove(playerIndex);

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

            var response = drawDestinationCardMove.Execute(game);
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

            var response = chooseDestinationCardMove.Execute(game);
            return response;
        }

        public MakeMoveResponse GetGameOutcome()
        {
            if (game.GameState != GameState.Ended)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.GameNotEndedYet
                };
            }

            var winners = GetWinner();

            return new GetGameWinnerResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.Ok,
                Winners = winners,
                Players = game.Players,
                LongestContPathLength = game.LongestContPathLength,
                LongestContPathPlayerIndex = game.LongestContPathPlayerIndex
            };
        }


        public List<Player> GetWinner()
        {
            var mostPoints = game.Players.Select(p => p.Points).Max();

            var playersWithMostPoints = game.Players.Where(p => p.Points == mostPoints).ToList();

            if (playersWithMostPoints.Count == 1)
            {
                return playersWithMostPoints;
            }

            var mostCompletedDestinations = playersWithMostPoints.Select(p => p.CompletedDestinationCards.Count).Max();

            var playersWithMostDestinationCards = playersWithMostPoints.Where(p => p.CompletedDestinationCards.Count == mostCompletedDestinations).ToList();

            if (playersWithMostDestinationCards.Count == 1)
            {
                return playersWithMostDestinationCards;
            }

            return playersWithMostDestinationCards;
        }

        #region AI
        public MakeMoveResponse MakeBotMove(int playerIndex)
        {
            var player = game.GetPlayer(playerIndex);

            var allPossibleMoves = GetAllPossibleMoves(playerIndex);

            if (allPossibleMoves.AllPossibleMovesCount == 0)
            {
                //end game
                game.EndGame();

                return new MakeMoveResponse
                {
                    IsValid = true, //or false?
                    Message = ValidMovesMessages.GameHasEndedNoMovesLeft
                };
            }

            if (player is BotPlayer bot)
            {
                return GetBotMove(bot, allPossibleMoves);
            }
            else
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.PlayerNotBot
                };
            }
        }

        public PossibleMoves GetAllPossibleMoves(int playerIndex)
        {
            if (game.GameState == GameState.ChoosingDestinationCards)
            {
                //get all choose destination card moves and return
                var moves = GetChooseDestinationCardMoves(playerIndex);
                return new PossibleMoves(
                    new List<DrawTrainCardMove>(),
                    new List<ClaimRouteMove>(),
                    null,
                    moves);
            }

            var listOfDrawTrainCardMoves = new List<DrawTrainCardMove>();
            var listOfClaimRouteMoves = new List<ClaimRouteMove>();

            if (game.GameState == GameState.WaitingForPlayerMove ||
                game.GameState == GameState.DecidingAction ||
                game.GameState == GameState.DrawingTrainCards)
            {
                listOfDrawTrainCardMoves.AddRange(GetAllPossibleDrawTrainCardMoves(playerIndex));
            }

            //add claimRouteMoves
            if (game.GameState == GameState.WaitingForPlayerMove ||
                    game.GameState == GameState.DecidingAction)
            {
                listOfClaimRouteMoves.AddRange(GetAllClaimRouteMoves(playerIndex));
            }

            DrawDestinationCardMove? destinationCardMove = null;

            if (game.GameState == GameState.WaitingForPlayerMove ||
                    game.GameState == GameState.DecidingAction)
            {
                destinationCardMove = GetDrawDestinationCardMove(playerIndex);
            }

            return new PossibleMoves(listOfDrawTrainCardMoves, listOfClaimRouteMoves, destinationCardMove, new List<ChooseDestinationCardMove>());
        }

        public List<ClaimRouteMove> GetAllClaimRouteMoves(int playerIndex)
        {
            var possibleClaimRouteMoves = new HashSet<ClaimRouteMove>();
            //go through all of them and see whether they can be claimed?
            var distinctRoutes = game.Board.Routes.Routes.Distinct();

            foreach (var route in distinctRoutes)
            {
                //see whether they can be claimed
                var canClaimRouteMove = new CanClaimRouteMove(playerIndex, route);

                var canRouteBeClaimed = moveValidatorService.CanRouteBeClaimed(canClaimRouteMove);

                if (!canRouteBeClaimed.IsValid)
                {
                    continue;
                }

                var response = canClaimRouteMove.Execute(game);
                if (!response.IsValid)
                {
                    //not enough resources
                    continue;
                }

                if (response is CanClaimRouteResponse canClaimRouteResponse)
                {
                    foreach (var color in canClaimRouteResponse.TrainColorsWhichCanBeUsed)
                    {
                        possibleClaimRouteMoves.Add(CreateClaimRouteMove(playerIndex, color, route));
                    }
                }
            }

            return possibleClaimRouteMoves.ToList();
        }

        public List<ChooseDestinationCardMove> GetChooseDestinationCardMoves(int playerIndex)
        {
            //get all cards marked as waiting to be chosen
            var destinationCardsToChooseFrom = game.Board.DestinationCards.Where(d => d.IsWaitingToBeChosen).ToList();

            var combinationsOfChosenCards = CombinationGenerator.GetCombinations(destinationCardsToChooseFrom, 1, destinationCardsToChooseFrom.Count);

            var moves = new List<ChooseDestinationCardMove>();

            foreach (var chosenCards in combinationsOfChosenCards)
            {
                var notChosenCards = destinationCardsToChooseFrom.Except(chosenCards).ToList();
                var move = new ChooseDestinationCardMove(playerIndex, chosenCards, notChosenCards);
                moves.Add(move);
            }

            return moves;
        }

        public MakeMoveResponse RunGame()
        {
            foreach (var player in game.Players)
            {
                if (!player.IsBot)
                {
                    return new MakeMoveResponse
                    {
                        IsValid = false,
                        Message = InvalidMovesMessages.NotAllPlayersAreBots
                    };
                }
            }

            var playerTurn = 0;
            string message = string.Empty;

            while (game.GameState != GameState.Ended)
            {
                lock (gameLock)
                {
                    playerTurn = game.PlayerTurn;
                    var moveResponse = MakeBotMove(playerTurn);
                    if (!moveResponse.IsValid)
                    {
                        return moveResponse;
                    }

                    if (moveResponse.Message == ValidMovesMessages.GameHasEndedNoMovesLeft)
                    {
                        message = moveResponse.Message;
                        break;
                    }
                }

            }

            if (message != string.Empty)
            {
                message = ValidMovesMessages.GameHasEnded;
            };

            return new RunGameResponse(
                true,
                message,
                game.Players,
                GetWinner(),
                game.LongestContPathLength,
                game.LongestContPathPlayerIndex);
        }
        #endregion
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

            var canClaimRouteMove = new CanClaimRouteMove(0, originCity, destinationCity);

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

            if (!IsPlayerIndexValid(move.PlayerIndex))
            {
                return InvalidMovesMessages.InvalidPlayerIndex;
            }

            if (move.PlayerIndex != game.PlayerTurn)
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

            var newGame = new Game(game.Board, players, game.GameState, game.PlayerTurn, game.GameLog);

            return newGame;
        }

        private DestinationCard? GetDestinationCard(City originCity, City destinationCity)
        {
            return game.Board.DestinationCards
                .Where(d => (d.Origin == originCity && d.Destination == destinationCity)
            || (d.Destination == originCity && d.Origin == destinationCity))
                .FirstOrDefault();
        }

        private MakeMoveResponse GetBotMove(BotPlayer bot, PossibleMoves possibleMoves)
        {
            lock (gameLock)
            {
                var move = bot.GetNextMove(game, possibleMoves);

                var canMakeMoveMessage = CanMakeMove(move);

                if (canMakeMoveMessage != ValidMovesMessages.Ok)
                {
                    return new MakeMoveResponse
                    {
                        IsValid = false,
                        Message = canMakeMoveMessage
                    };
                }
                var response = move.Execute(game);

                return response;
            }
        }

        private List<DrawTrainCardMove> GetAllPossibleDrawTrainCardMoves(int playerIndex)
        {
            var drawTrainCardMoves = new List<DrawTrainCardMove>();

            //draw from face down deck
            var drawBlindCardMove = new DrawTrainCardMove(playerIndex, -1);
            if (moveValidatorService.ValidateDrawTrainCardMove(drawBlindCardMove).IsValid)
            {
                drawTrainCardMoves.Add(drawBlindCardMove);
            }

            for (int i = 0; i < game.Board.FaceUpDeck.Count; i++)
            {
                var drawFaceUpCardMove = new DrawTrainCardMove(playerIndex, i);

                if (moveValidatorService.ValidateDrawTrainCardMove(drawFaceUpCardMove).IsValid)
                {
                    drawTrainCardMoves.Add(drawFaceUpCardMove);
                }
            }

            return drawTrainCardMoves;
        }

        private DrawDestinationCardMove? GetDrawDestinationCardMove(int playerIndex)
        {
            var drawDestinationCardMove = new DrawDestinationCardMove(playerIndex);

            var validateMove = moveValidatorService.ValidateDrawDestinationCardsMove(drawDestinationCardMove);
            if (validateMove.IsValid)
            {
                return drawDestinationCardMove;
            }
            else
            {
                return null;
            }
        }

        private ClaimRouteMove CreateClaimRouteMove(int playerIndex, TrainColor color, Model.GameBoard.Route route)
        {
            return new ClaimRouteMove(playerIndex, color, route);
            //don't validate it, it's already valid
        }


    }
}
