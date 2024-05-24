using TicketToRide.Moves;

namespace TicketToRide.Controllers.Responses
{
    public class GetAllPossibleMovesResponse : MakeMoveResponse
    {
        List<Move> AllPossibleMoves { get; set; }
    }
}
