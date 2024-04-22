﻿using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class CanClaimRouteMove : Move
    {
        public City Origin { get; set; }

        public City Destination {  get; set; }

        public Model.GameBoard.Route Route {  get; set; }

        public CanClaimRouteMove(Game game, int playerIndex, City origin, City destination) 
            : base(game, playerIndex)
        {
            this.Origin = origin;
            this.Destination = destination;
        }

        public override MakeMoveResponse Execute()
        {
            //change game state
            if (Game.GameState == GameState.WaitingForPlayerMove)
            {
                Game.GameState = GameState.DecidingAction;
            }

            //check that the player can claim this route
            var colorsWithWhichRouteCanBeClaimed = Game.Board.Routes
                .ColorsWithWhichRouteCanBeClaimed(Route, Game.Players.ElementAt(playerIndex));

            if (colorsWithWhichRouteCanBeClaimed.Count == 0)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.PlayerDoesNotHaveResources
                };
            }
            else
            {
                return new CanClaimRouteResponse
                {
                    IsValid = true,
                    Message = ValidMovesMessages.PlayerCanClaimRoute,
                    TrainColorsWhichCanBeUsed = colorsWithWhichRouteCanBeClaimed,
                    Route = Route
                };
            }
        }
    }
}