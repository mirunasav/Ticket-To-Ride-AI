using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Text.Json;
using TicketToRide.Controllers.Requests;
using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Services;

namespace TicketToRide.Controllers
{
    public class GameController : Controller
    {
        private readonly GameService gameService;

        private readonly RouteService routeService;

        public GameController(GameService gameService, RouteService routeService)
        {
            this.gameService = gameService;
            this.routeService = routeService;
        }

        [HttpPost]
        public IActionResult NewGame([FromBody] NewGameRequest newGameRequest)
        {
            if (newGameRequest.NumberOfPlayers < 2 || newGameRequest.NumberOfPlayers > 4)
            {
                return BadRequest("Number of Players needs to be between 2 and 4");
            }

            var playerTypes = new List<PlayerType>();

            if (newGameRequest.PlayerTypes != null && newGameRequest.PlayerTypes.Count > 0)
            {
                if (newGameRequest.PlayerTypes.Count != newGameRequest.NumberOfPlayers)
                {
                    return BadRequest("Invalid number of player types provided");
                }

                foreach (var type in newGameRequest.PlayerTypes)
                {
                    bool typeExists = Enum.TryParse(type, out PlayerType playerType);

                    if (!typeExists)
                    {
                        return BadRequest("Invalid playerType");
                    }
                    playerTypes.Add(playerType);
                }
            }
            else
            {
                for (int i = 0; i < newGameRequest.NumberOfPlayers; i++)
                {
                    playerTypes.Add(PlayerType.Human);
                }
            }

            var game = gameService.InitializeGame(newGameRequest.NumberOfPlayers, playerTypes);

            return Json(game);
        }

        [HttpGet]
        public IActionResult Index(int playerIndex)
        {
            //pasez player index si iau datele pt fiecare player, restul au hide data
            try
            {

                var game = gameService.GetGameInstance(playerIndex);

                if (game is null)
                {
                    return NotFound("Game is null");
                }

                return Json(game);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest("Invalid player index.");
            }
        }

        [HttpGet]
        public IActionResult GetGame()
        {

            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return NotFound("Game is null");
            }

            return Json(game);
        }

        [HttpGet]
        public IActionResult LoadGameForReplay([FromQuery] string guid)
        {
            //get path
            string initialGameStatePath = GameConstants.InitialGameStatesDirectoryPath; // Replace with your actual directory path
            string gameLogPath = GameConstants.GameLogDirectoryPath;
            string trainCardDeckPath = GameConstants.TrainCardsStatesDirectoryPath;

            string gameStateFileName = $"GameState{guid}.json";

            string gameLogFileName = $"GameLog{guid}.txt";

            string trainCardDeckFileName = $"TrainCardsStates{guid}.json";

            // Combine the directory path with the filename to get the full file path
            string initialGameFilePath = Path.Combine(initialGameStatePath, gameStateFileName);
            string gameLogFilePath = Path.Combine(gameLogPath, gameLogFileName);
            string trainCardDeckFilePath = Path.Combine(trainCardDeckPath, trainCardDeckFileName);

            if (!System.IO.File.Exists(initialGameFilePath))
            {
                return NotFound($"The file {initialGameFilePath} does not exist.");
            }


            if (!System.IO.File.Exists(gameLogFilePath))
            {
                return NotFound($"The file {gameLogFilePath} does not exist.");
            }

            if (!System.IO.File.Exists(trainCardDeckFilePath))
            {
                return NotFound($"The file {trainCardDeckFilePath} does not exist.");
            }

            var reloadableGame = gameService.PrepGameForReplay(initialGameFilePath, gameLogFilePath, trainCardDeckFilePath);

            return Json(reloadableGame.Game);
        }

        [HttpGet]
        public IActionResult MakeNextMove()
        {
            var response = gameService.MakeNextMove();

            if (response.IsValid)
            {
                return Json(gameService.GetGameInstance());
            }

            return BadRequest();
        }

        [HttpGet]
        public IActionResult Replay([FromQuery] string guid)
        {
            //get path
            string initialGameStatePath = GameConstants.InitialGameStatesDirectoryPath; // Replace with your actual directory path
            string gameLogPath = GameConstants.GameLogDirectoryPath;
            string trainCardDeckPath = GameConstants.TrainCardsStatesDirectoryPath;

            string gameStateFileName = $"GameState{guid}.json";

            string gameLogFileName = $"GameLog{guid}.txt";

            string trainCardDeckFileName = $"TrainCardsStates{guid}.json";

            // Combine the directory path with the filename to get the full file path
            string initialGameFilePath = Path.Combine(initialGameStatePath, gameStateFileName);
            string gameLogFilePath = Path.Combine(gameLogPath, gameLogFileName);
            string trainCardDeckFilePath = Path.Combine(trainCardDeckPath, trainCardDeckFileName);

            if (!System.IO.File.Exists(initialGameFilePath))
            {
                return NotFound($"The file {initialGameFilePath} does not exist.");
            }


            if (!System.IO.File.Exists(gameLogFilePath))
            {
                return NotFound($"The file {gameLogFilePath} does not exist.");
            }

            if (!System.IO.File.Exists(trainCardDeckFilePath))
            {
                return NotFound($"The file {trainCardDeckFilePath} does not exist.");
            }

            var gameResult = gameService.Replay(initialGameFilePath, gameLogFilePath, trainCardDeckFilePath);

            if (gameResult is RunGameResponse response)
            {
                return Json(new
                {
                    gameLogFileName = response.GameLogFile,
                    initialGameStateFileName = response.InitialGameStateFile,
                    trainCardDeckStatesFileName = response.TrainCardDeckStatesFileName,
                    winners = response.Winners,
                    players = response.Players,
                    longestContPathLength = response.LongestContPathLength,
                    longestContPathPlayerIndex = response.LongestContPathPlayerIndex
                });
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult Route(string originCity, string destinationCity)
        {
            bool originExists = Enum.TryParse(originCity, out City origin);
            bool destinationExists = Enum.TryParse(destinationCity, out City destination);

            if (!originExists || !destinationExists)
            {
                return BadRequest("Origin or destination city does not exist");
            }

            var route = routeService.GetRoute(origin, destination);

            if (route is null)
            {
                return NotFound("Route does not exist");
            }

            return Json(route);
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            gameService.DeleteGame();

            return Ok();
        }

        [HttpGet]
        public IActionResult IsGameInitialized()
        {
            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return Json(false);
            }

            return Json(true);
        }

        [HttpGet]
        public IActionResult GetPlayers()
        {
            return Json(gameService.GetPlayers());
        }

        [HttpGet]
        public IActionResult DrawTrainCard(int playerIndex, int faceUpCardIndex = -1)
        {
            var response = gameService.DrawTrainCard(playerIndex, faceUpCardIndex);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            var game = gameService.GetGameInstance();
            return Json(new
            {
                Message = response.Message,
                playerHand = game.Players[playerIndex].Hand,
                faceUpDeck = game.Board.FaceUpDeck,
                gameState = game.GameState
            });
        }

        //click on 2 cities; see whether that route can be claimed
        [HttpGet]
        public IActionResult CanClaimRoute([FromQuery] int playerIndex, string origin, string destination)
        {
            bool originExists = Enum.TryParse(origin, out City originCity);
            bool destinationExists = Enum.TryParse(destination, out City destinationCity);

            if (!originExists || !destinationExists)
            {
                return BadRequest("Origin or destination city does not exist");
            }

            var response = gameService.CanClaimRoute(new CanClaimRouteRequest
            {
                PlayerIndex = playerIndex,
                Origin = originCity,
                Destination = destinationCity
            });

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is CanClaimRouteResponse canClaimRouteResponse)
            {
                return Json(new
                {
                    Message = canClaimRouteResponse.Message,
                    trainColors = canClaimRouteResponse.TrainColorsWhichCanBeUsed,
                    route = canClaimRouteResponse.Route
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult ClaimRoute([FromBody] ClaimRouteRequest request)
        {
            bool originExists = Enum.TryParse(request.OriginCity, out City originCity);
            bool destinationExists = Enum.TryParse(request.DestinationCity, out City destinationCity);
            bool trainColorExists = Enum.TryParse(request.ColorUsed, out TrainColor color);

            if (!originExists || !destinationExists)
            {
                return BadRequest("Origin or destination city does not exist");
            }

            if (!trainColorExists)
            {
                return BadRequest("This train color does not exist");
            }

            var response = gameService.ClaimRoute(request.PlayerIndex, originCity, destinationCity, color);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is ClaimRouteResponse claimRouteResponse)
            {
                return Json(new
                {
                    Message = claimRouteResponse.Message,
                    playerRemainingCards = claimRouteResponse.PlayerRemainingCards,
                    newlyCompletedDestinations = claimRouteResponse.NewlyCompletedDestinations
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public IActionResult DrawDestinationCards(int playerIndex)
        {
            var response = gameService.DrawDestinationCards(playerIndex);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is DrawDestinationCardsResponse drawDestinationCardResponse)
            {
                return Json(new
                {
                    Message = drawDestinationCardResponse.Message,
                    DrawnDestinationCards = drawDestinationCardResponse.DrawnDestinationCards
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult DrawDestinationCards([FromBody] DrawDestinationCardsRequest request)
        {
            var response = gameService.ChooseDestinationCards(request);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            return Json(new
            {
                Message = response.Message
            });
        }

        [HttpGet]
        public IActionResult GetGameOutcome()
        {
            var response = gameService.GetGameOutcome();

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            if (response is GetGameWinnerResponse getGameWinnerResponse)
            {
                return Json(new
                {
                    Message = getGameWinnerResponse.Message,
                    Winners = getGameWinnerResponse.Winners,
                    Players = getGameWinnerResponse.Players,
                    LongestContPathLength = getGameWinnerResponse.LongestContPathLength,
                    LongestContPathPlayerIndex = getGameWinnerResponse.LongestContPathPlayerIndex
                });
            }
            else
            {
                return BadRequest();
            }
        }


        #region AI
        [HttpGet]
        public IActionResult MakeBotMove(int playerIndex)
        {
            var response = gameService.MakeBotMove(playerIndex);

            if (!response.IsValid)
            {
                return BadRequest(response.Message);
            }

            return Json(response);
        }

        [HttpGet]
        public IActionResult GetAllPossibleMoves(int playerIndex)
        {
            var response = gameService.GetAllPossibleMoves(playerIndex);

            return Ok(response);
        }

        [HttpGet]
        public IActionResult GetAllPossibleClaimRouteMoves(int playerIndex)
        {
            var response = gameService.GetAllClaimRouteMoves(playerIndex);
            return Json(response);
        }

        [HttpGet]
        public IActionResult RunGame()
        {
            var gameResult = gameService.RunGame();

            if (!gameResult.IsValid)
            {
                return BadRequest(gameResult.Message);
            }

            if (gameResult is RunGameResponse response)
            {
                return Json(new
                {
                    gameLogFileName = response.GameLogFile,
                    initialGameStateFileName = response.InitialGameStateFile,
                    trainCardDeckStatesFileName = response.TrainCardDeckStatesFileName,
                    winners = response.Winners,
                    players = response.Players,
                    longestContPathLength = response.LongestContPathLength,
                    longestContPathPlayerIndex = response.LongestContPathPlayerIndex
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult RunMultipleGames([FromBody] NewMultipleGamesRequest newMultipleGamesRequest)
        {
            int numberOfGames = newMultipleGamesRequest.NumberOfGames;

            var gameSummaries = new MultipleGameResponse();

            if (newMultipleGamesRequest.NumberOfPlayers < 2 || newMultipleGamesRequest.NumberOfPlayers > 4)
            {
                return BadRequest("Number of Players needs to be between 2 and 4");
            }

            var playerTypes = new List<PlayerType>();

            if (newMultipleGamesRequest.PlayerTypes != null && newMultipleGamesRequest.PlayerTypes.Count > 0)
            {
                if (newMultipleGamesRequest.PlayerTypes.Count != newMultipleGamesRequest.NumberOfPlayers)
                {
                    return BadRequest("Invalid number of player types provided");
                }

                foreach (var type in newMultipleGamesRequest.PlayerTypes)
                {
                    bool typeExists = Enum.TryParse(type, out PlayerType playerType);

                    if (!typeExists)
                    {
                        return BadRequest("Invalid playerType");
                    }
                    playerTypes.Add(playerType);
                }
            }
            else
            {
                for (int i = 0; i < newMultipleGamesRequest.NumberOfPlayers; i++)
                {
                    playerTypes.Add(PlayerType.Human);
                }
            }

            for (int i = 0; i < numberOfGames; i++)
            {
                gameService.InitializeGame(newMultipleGamesRequest.NumberOfPlayers, playerTypes);
                var gameResult = gameService.RunGame();

                if (!gameResult.IsValid)
                {
                    var message = $"error at game {i} : {gameResult.Message}";
                    return BadRequest(message);
                }

                if (gameResult is RunGameResponse response)
                {
                    var gameSummaryResponse = gameService.ComputeGameSummaryResponse(response, gameSummaries.GameStatistics);
                    gameSummaries.GameSummaries.Add(gameSummaryResponse);
                }
            }

            gameSummaries.GameStatistics.ComputePlayerPointAverage(numberOfGames);
            gameSummaries.GameStatistics.ComputeAvgDestinationTickets(numberOfGames);

            gameSummaries.GameStatistics.routesClaimed = gameSummaries.GameStatistics.routesClaimed
               .OrderByDescending(kvp => kvp.Value)
               .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            gameSummaries.GameStatistics.numberOfRoutesClaimedForCity = gameSummaries.GameStatistics.numberOfRoutesClaimedForCity.
               OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            gameSummaries.GameStatistics.numberOfDestinationCardsInWinningGames = gameSummaries.GameStatistics.numberOfDestinationCardsInWinningGames.
              OrderByDescending(kvp => kvp.Value)
           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return Json(gameSummaries);
        }

        [HttpPost]
        public IActionResult RunNewGame([FromBody] NewGameRequest newGameRequest)
        {
            if (newGameRequest.NumberOfPlayers < 2 || newGameRequest.NumberOfPlayers > 4)
            {
                return BadRequest("Number of Players needs to be between 2 and 4");
            }

            var playerTypes = new List<PlayerType>();

            if (newGameRequest.PlayerTypes != null && newGameRequest.PlayerTypes.Count > 0)
            {
                if (newGameRequest.PlayerTypes.Count != newGameRequest.NumberOfPlayers)
                {
                    return BadRequest("Invalid number of player types provided");
                }

                foreach (var type in newGameRequest.PlayerTypes)
                {
                    bool typeExists = Enum.TryParse(type, out PlayerType playerType);

                    if (!typeExists)
                    {
                        return BadRequest("Invalid playerType");
                    }
                    playerTypes.Add(playerType);
                }
            }
            else
            {
                for (int i = 0; i < newGameRequest.NumberOfPlayers; i++)
                {
                    playerTypes.Add(PlayerType.Human);
                }
            }

            gameService.InitializeGame(newGameRequest.NumberOfPlayers, playerTypes);

            return RunGame();
        }
        #endregion

        #region frontend
        [HttpGet]
        public IActionResult DoesRouteExist([FromQuery] string origin, string destination)
        {
            var result = gameService.CanRouteBeClaimed(origin, destination);
            return Json(result);
        }

        [HttpGet]
        public IActionResult GetPlayerCompletedRoutes([FromQuery] int playerIndex)
        {
            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return NotFound("Game is null");
            }

            return Json(new
            {
                playerRoutes = game.Players.ElementAt(playerIndex).ClaimedRoutes
            });
        }

        [HttpGet]
        public IActionResult GetGameState()
        {
            var game = gameService.GetGameInstance();

            if (game is null)
            {
                return NotFound("Game is null");
            }

            return Json(new
            {
                gameState = game.GameState,
                playerTurn = game.PlayerTurn
            });
        }
        #endregion

    }
}
