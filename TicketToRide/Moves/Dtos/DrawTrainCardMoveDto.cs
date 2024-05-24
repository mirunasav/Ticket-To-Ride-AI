namespace TicketToRide.Moves.Dtos
{
    public class DrawTrainCardMoveDto : MoveDto
    {
        public int FaceUpCardIndex { get; set; } = 0;

        public DrawTrainCardMoveDto(int faceUpCardIndex)
        {
            FaceUpCardIndex = faceUpCardIndex;
        }

        public DrawTrainCardMoveDto(DrawTrainCardMove move)
        {
            FaceUpCardIndex = move.faceUpCardIndex;
        }
    }
}
