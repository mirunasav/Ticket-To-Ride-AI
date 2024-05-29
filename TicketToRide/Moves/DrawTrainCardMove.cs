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
        public DrawTrainCardMove(int playerIndex, int faceUpCardIndex) : base(playerIndex)
        {
            this.faceUpCardIndex = faceUpCardIndex;
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

                cardColor = card.Color.ToString();

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
                var card = game.Board.Deck.Pop(1);

                cardColor = card[0].Color.ToString();

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

            var gameLogMessage = CreateGameLogMessage(
                game.GetPlayer(PlayerIndex).Name, 
                cardColor,
                faceUpCardIndex != -1);

            LogMove(game.GameLog, gameLogMessage);
           
            return new MakeMoveResponse
            {
                IsValid = true,
                Message = ValidMovesMessages.PlayerDrawTrainCard
            };
        }

        private string CreateGameLogMessage(string playerName, string trainColor, bool isFromFaceUpDeck)
        {
            if (isFromFaceUpDeck)
            {
                return $"{playerName} has drawn a {trainColor} card from the face up deck.";
            }
            else
            {
                return $"{playerName} has drawn a card from the face down deck.";
            }
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
