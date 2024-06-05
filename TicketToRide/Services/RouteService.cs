using System.Net.WebSockets;
using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
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

        public MakeMoveResponse CanPlayerClaimRoute(ClaimRouteMove claimRouteMove, int numberOfPlayers)
        {
            var game = gameProvider.GetGame();

            var player = game.Players.ElementAt(claimRouteMove.PlayerIndex);

            //vad daca ruta e claimed
            if (IsRouteClaimed(claimRouteMove.Route, numberOfPlayers))
            {
                //logica in plus pt jocuri cu 3-4 pers?
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.RouteAlreadyClaimed
                };
            }

            var length = claimRouteMove.Route.ElementAt(0).Length;

            // vad daca are suficiente trenuri
            if (player.RemainingTrains < length)
            {
                return new MakeMoveResponse
                {
                    IsValid = false,
                    Message = InvalidMovesMessages.NotEnoughTrainPieces
                };
            }

            //vad daca are cartonasele necesare
            var trainColorsWhichCanBeUsed = ColorsWithWhichRoutesCanBeClaimed(claimRouteMove.Route, player);

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

        public List<Model.GameBoard.Route> GetRoute(City origin, City destination)
        {
            return gameProvider.GetGame().Board.Routes.GetRoute(origin, destination);
        }

        public bool IsRouteClaimed(IList<Model.GameBoard.Route> routeCollection, int numberOfPlayers)
        {
            //if the route is not double, just check if the first element of the collection 
            //is claimed
            if (routeCollection.Count == 1)
            {
                return routeCollection.ElementAt(0).IsClaimed;
            }

            //else, check if any of the routes is claimed and the game has more than 3 players

            var claimedRoutes = routeCollection.Where(r => r.IsClaimed).ToList();
            
            foreach(var claimedRoute in claimedRoutes)
            {
                routeCollection.Remove(claimedRoute);
            }

            if (claimedRoutes.Count == 1 && numberOfPlayers < GameConstants.MinNumberOfPlayersForWhichDoubleRoutesCanBeUsed)
            {
                return true;
            }

            if(claimedRoutes.Count == 2 && numberOfPlayers >= GameConstants.MinNumberOfPlayersForWhichDoubleRoutesCanBeUsed)
            {
                return true;
            }

            return false;
        }

        public bool DoesRouteExist(Model.GameBoard.Route route)
        {
            return gameProvider.GetGame().Board.Routes.GetRoute(route.Origin, route.Destination) != null;
        }

        public IList<DestinationCard> GetNewlyCompletedDestinations(Player player)
        {
            var newlyCompletedDestinations = new List<DestinationCard>();

            foreach(var card in player.PendingDestinationCards)
            {
                var isCompleted = player.ClaimedRoutes.AreConnected(card.Origin, card.Destination);

                if (isCompleted)
                {
                    player.MarkDestinationAsCompleted(card);
                    newlyCompletedDestinations.Add(card);
                }
            }

            return newlyCompletedDestinations;
        }

        public IList<TrainColor> ColorsWithWhichRoutesCanBeClaimed(IList<Model.GameBoard.Route> routes, Player player)
        {
            if (routes == null || routes.Count == 0)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            var cardsWhichCanBeUsed = new List<TrainColor>();
            var locomotiveCount = player.GetLocomotiveCount();

            foreach (var route in routes)
            {
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
            }
            return cardsWhichCanBeUsed.Distinct().ToList() ;
        }
    }
}
