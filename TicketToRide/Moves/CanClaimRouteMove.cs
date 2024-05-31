using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class CanClaimRouteMove : Move
    {
        public City Origin { get; set; }

        public City Destination { get; set; }

        public List<Model.GameBoard.Route> Route { get; set; }

        public CanClaimRouteMove(int playerIndex, City origin, City destination)
            : base(playerIndex)
        {
            this.Origin = origin;
            this.Destination = destination;
        }

        public CanClaimRouteMove(int playerIndex, List<Model.GameBoard.Route> route)
             : base(playerIndex)
        {
            this.Origin = route.ElementAt(0).Origin;
            this.Destination = route.ElementAt(0).Destination;
            Route = [.. route];
        }

        public override MakeMoveResponse Execute(Game game)
        {
            var colorsWithWhichRoutesCanBeClaimed = new List<TrainColor>();

            foreach (var route in Route)
            {
                //check that the player can claim this route
                var possibleColors = game.Board.Routes
                    .ColorsWithWhichRouteCanBeClaimed(route, game.Players.ElementAt(PlayerIndex));

                colorsWithWhichRoutesCanBeClaimed.AddRange(possibleColors);
            }

            if (colorsWithWhichRoutesCanBeClaimed.Count == 0)
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
                    TrainColorsWhichCanBeUsed = colorsWithWhichRoutesCanBeClaimed.Distinct().ToList(),
                    Route = Route
                };
            }
        }
    }
}

