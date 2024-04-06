﻿using TicketToRide.Controllers.Responses;
using TicketToRide.Model.GameBoard;
using TicketToRide.Model.Players;

namespace TicketToRide.Moves
{
    public abstract class Move
    {
        public Game Game { get; set; }

        public int playerIndex { get; set; }

        public abstract MakeMoveResponse ValidateMove();

        public Move(Game game, int playerIndex)
        {
            this.Game = game;
            this.playerIndex = playerIndex;
        }

        public abstract MakeMoveResponse Execute();

    }
}