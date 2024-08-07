﻿using TicketToRide.GameLogs;
using TicketToRide.Model.Enums;
using TicketToRide.Model.Players;

namespace TicketToRide.Model.GameBoard
{
    public class Game
    {
        public Board Board { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public int PlayerTurn { get; set; } = 0;

        public int GameTurn { get; set; } = 1;

        public GameState GameState { get; set; } = GameState.DrawingFirstDestinationCards;

        public bool IsFinalTurn { get; set; } = false;

        public bool IsGameAReplay { get; set; } = false;

        public int LongestContPathLength { get; set; }

        public int LongestContPathPlayerIndex { get; set; }

        private int LastPlayerTurn { get; set; } = 0;

        public GameLog GameLog { get; set; }

        public Game()
        {

        }

        public Game(Board board, List<Player> players, GameLog gameLog)
        {
            this.Board = board;
            this.Players = players;
            this.GameLog = gameLog;
        }

        //for player POV
        public Game(Board board,
            List<Player> players,
            GameState gameState,
            int playerTurn,
            GameLog gameLog,
            bool isGameAReplay
            )
        {
            Board = board;
            Players = players;
            GameState = gameState;
            PlayerTurn = playerTurn;
            GameLog = gameLog;
            IsGameAReplay = isGameAReplay;
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
            GameState = GameState.WaitingForPlayerMove;
            
            if (PlayerTurn == LastPlayerTurn && IsFinalTurn)
            {
                EndGame();
                return;
            }

            if (!IsFinalTurn)
            {
                UpdateIsFinalTurn();
            }
            ChangePlayerTurn();
        }

        public Player GetPlayer(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= Players.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex));
            }

            return Players.ElementAt(playerIndex);
        }

        public string GetPlayerName(int playerIndex)
        {
            return Players.ElementAt(playerIndex).Name;
        }

        public Route MarkRouteAsClaimed(City origin, City destination, Player player, TrainColor colorUsed)
        {
            var foundRoute = Board.Routes.GetRoute(origin, destination, colorUsed, true);

            ////find out if the route is a double route
            //var doubleRoutes = Board.Routes.GetRoute(origin, destination);

            //if (doubleRoutes.Count > 1)
            //{
            //    //is double route, so check if the game is multiplayer
            //    var numberOfPlayers = Players.Count;
            //    if (numberOfPlayers < GameConstants.MinNumberOfPlayersForWhichDoubleRoutesCanBeUsed)
            //    {
            //        //remove both edges from route graph
            //        Board.RouteGraph.RemoveEdges(doubleRoutes);
            //    }
            //    else
            //    {
            //        Board.RouteGraph.RemoveEdge(foundRoute);
            //    }
            //}
            ////otherwise, just remove the found route
            //else
            //{
            //    Board.RouteGraph.RemoveEdge(foundRoute);
            //}

            ArgumentNullException.ThrowIfNull(nameof(foundRoute));

            foundRoute.IsClaimed = true;
            foundRoute.ClaimedBy = player.Color;
            return foundRoute;
        }

        public void EndGame()
        {
            if (GameState != GameState.Ended)
            {
                ComputeFinalPoints();
                GameState = GameState.Ended;
            }
        }
      
        public void TryBeginGame()
        {
            var canBeginGame = true;

            foreach (var player in Players)
            {
                if (player.PendingDestinationCards is null ||
                    player.PendingDestinationCards.Count == 0)
                {
                    canBeginGame = false;
                    break;
                }
            }

            if (canBeginGame)
            {
                GameState = GameState.WaitingForPlayerMove;
                GameTurn = 0;
            }
            else
            {
                GameState = GameState.DrawingFirstDestinationCards;
            }

            ChangePlayerTurn();
        }

        #region private
        private void ChangePlayerTurn()
        {
            if (PlayerTurn == Players.Count - 1)
            {
                PlayerTurn = 0;
                GameTurn++;
                return;
            }

            PlayerTurn += 1;
        }

        private void UpdateIsFinalTurn()
        {
            foreach (var player in Players)
            {
                if (player.RemainingTrains <= 2)
                {
                    IsFinalTurn = true;
                    LastPlayerTurn = PlayerTurn;
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
                if (longestContPath.Item1 > longestContPathLength)
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
