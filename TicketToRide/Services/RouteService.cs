using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;
using TicketToRide.Model.Players;
using TicketToRide.Moves;

namespace TicketToRide.Services
{
    public class RouteService
    {
        private readonly GameProvider gameProvider;

        public RouteService(GameProvider gameProvider) 
        {
            this.gameProvider = gameProvider;
        }

        public MakeMoveResponse CanPlayerClaimRoute(ClaimRouteMove claimRouteMove)
        {
            var game = gameProvider.GetGame();

            var player = game.Players.ElementAt(claimRouteMove.playerIndex);

            //vad daca ruta e claimed
            if (claimRouteMove.Route.IsClaimed)
            {
                //logica in plus pt jocuri cu 3-4 pers?
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteAlreadyClaimed
                };
            }

            // vad daca are suficiente trenuri
            if (player.RemainingTrains < claimRouteMove.Route.Length)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.NotEnoughTrainPieces
                };
            }

            //vad daca are cartonasele necesare
            var trainColorsWhichCanBeUsed = ColorsWithWhichRouteCanBeClaimed(claimRouteMove.Route, player);

            if (trainColorsWhichCanBeUsed.Count == 0)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.PlayerDoesNotHaveResources
                };
            }

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerCanClaimRoute
            };
        }
       
        public Model.GameBoard.Route GetRoute(City origin, City destination)
        {
            return gameProvider.GetGame().Board.Routes.GetRoute(origin, destination);
        }

        public bool DoesRouteExist(Model.GameBoard.Route route)
        {
            return gameProvider.GetGame().Board.Routes.GetRoute(route.Origin, route.Destination) != null ;
        }

        public IList<TrainColor> ColorsWithWhichRouteCanBeClaimed(Model.GameBoard.Route route, Player player)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            var cardsWhichCanBeUsed = new List<TrainColor>();
            var locomotiveCount = player.GetLocomotiveCount();

            if (route.Color == TrainColor.Grey)
            {
                var groupedCardsByColor = player.Hand
                     .Where(card => card.Color != TrainColor.Locomotive)
                    .GroupBy(card => card.Color);

                var groupsOfValidColors = groupedCardsByColor
                    .Where(group => group.Count() + locomotiveCount >= route.Length).ToList();

                //return the colors of cards which can be used
                foreach (var group in groupsOfValidColors)
                {
                    cardsWhichCanBeUsed.Add(group.Key);
                }
            }
            else
            {
                var playerTrainsOfNecessaryColor = player.GetCardsOfColor(route.Color);

                playerTrainsOfNecessaryColor += locomotiveCount;

                if (playerTrainsOfNecessaryColor >= route.Length)
                {
                    cardsWhichCanBeUsed.Add(route.Color);
                }
            }

            return cardsWhichCanBeUsed;
        }
    }
}
