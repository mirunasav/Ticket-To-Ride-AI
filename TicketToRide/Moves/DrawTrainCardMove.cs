using TicketToRide.Controllers.Responses;
using TicketToRide.Model.Cards;
using TicketToRide.Model.Constants;
using TicketToRide.Model.Enums;
using TicketToRide.Model.GameBoard;

namespace TicketToRide.Moves
{
    public class DrawTrainCardMove : Move
    {
        public int faceUpCardIndex { get; set; } = 0;

        public TrainColor CardColor { get; set; } = default;

        public DrawTrainCardMove(int playerIndex, int faceUpCardIndex, TrainColor cardColor = default) : base(playerIndex)
        {
            this.faceUpCardIndex = faceUpCardIndex;
            CardColor = cardColor;
        }

        public override MakeMoveResponse Execute(Game game)
        {
            bool isTurnFinishedByLocomotiveDraw = false;
            string cardColor;

            if (faceUpCardIndex != -1)
            {
                //draw from face up card
                var card = game.Board.FaceUpDeck.ElementAt(faceUpCardIndex);
                game.Board.FaceUpDeck.Remove(card);

                CardColor = card.Color;

                //add to player hand
                game.GetPlayer(PlayerIndex).Hand.Add(card);

                //refill faceup deck
                game.Board.PopulateFaceUpDeck();

                //if the drawn card was a locomotive, will change state to finished state
                isTurnFinishedByLocomotiveDraw = card.Color == TrainColor.Locomotive;
            }

            //choose to draw from deck
            else
            {
                //take first card from deck
                if(game.Board.Deck.Count == 0)
                {
                    game.Board.RefillDeck();
                }
                var card = game.Board.Deck.Pop(1);

                //add to player hand
                game.GetPlayer(PlayerIndex).Hand.AddRange(card);
            }

            var newGameState = UpdateGameState(game, isTurnFinished: isTurnFinishedByLocomotiveDraw);

            //if the player still has to draw cards
            //make the locomotives unavailable
            if(newGameState == GameState.DrawingTrainCards)
            {
                game.Board.ChangeFaceUpLocomotiveStatus(isAvailable: false);
            }

            //if the player's turn has finished, change status of locomotives to available
            else if (newGameState == GameState.WaitingForPlayerMove)
            {
                game.Board.ChangeFaceUpLocomotiveStatus(isAvailable: true);
                game.UpdateStateNextPlayerTurn();
            }

            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerDrawTrainCard
            };
        }

        private GameState UpdateGameState(Game game, bool isTurnFinished = false)
        {
            //drawn a locomotive card, OR
            //already drawn one card, so after this one, the player's turn will end

            if (isTurnFinished || game.GameState == GameState.DrawingTrainCards)
            {
                return GameState.WaitingForPlayerMove;
            }

            //the first card from this player's drawing cards turn
            else if (game.GameState == GameState.WaitingForPlayerMove)
            {
                game.GameState = GameState.DrawingTrainCards;
            }

            return game.GameState;
        }
    }
}
