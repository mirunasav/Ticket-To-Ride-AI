using TicketToRide.Moves.Dtos;

namespace TicketToRide.Moves
{
    public class PossibleMovesDto
    {
        public List<DrawTrainCardMoveDto> DrawTrainCardMoves { get; set; }

        public List<ClaimRouteMoveDto> ClaimRouteMoves { get; set; }

        public DrawDestinationCardMoveDto? DrawDestinationCardMove { get; set; }

        public PossibleMovesDto(
            List<DrawTrainCardMoveDto> drawTrainCards,
            List<ClaimRouteMoveDto> claimRoutes,
            DrawDestinationCardMoveDto drawDestinationCard) 
        { 
            DrawTrainCardMoves = drawTrainCards;
            ClaimRouteMoves = claimRoutes;
            DrawDestinationCardMove = drawDestinationCard;
        }
    }
}
