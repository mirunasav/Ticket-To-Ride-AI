﻿namespace TicketToRide.Moves
{
    public class PossibleMoves
    {
        public List<DrawTrainCardMove> DrawTrainCardMoves { get; set; }

        public List<ClaimRouteMove> ClaimRouteMoves { get; set; }

        public DrawDestinationCardMove? DrawDestinationCardMove { get; set; }

        public List<ChooseDestinationCardMove> ChooseDestinationCardMoves { get; set; }

        public int AllPossibleMovesCount { get; set; }

        public bool CanDestinationCardBeTheOnlyMove {  get; set; }

        public PossibleMoves(
            List<DrawTrainCardMove> drawTrainCards,
            List<ClaimRouteMove> claimRoutes,
            DrawDestinationCardMove? drawDestinationCard,
            List<ChooseDestinationCardMove> chooseDestinationCardMoves,
            bool canDestinationCardBeTheOnlyMove = false)
        { 
            DrawTrainCardMoves = drawTrainCards;
            ClaimRouteMoves = claimRoutes;
            DrawDestinationCardMove = drawDestinationCard;
            ChooseDestinationCardMoves = chooseDestinationCardMoves;
            CanDestinationCardBeTheOnlyMove = canDestinationCardBeTheOnlyMove;

            AllPossibleMovesCount = drawTrainCards.Count + claimRoutes.Count + chooseDestinationCardMoves.Count;

            if (drawDestinationCard != null) 
            {
                if(canDestinationCardBeTheOnlyMove)
                {
                    AllPossibleMovesCount++;
                }
                else
                {
                    if (drawTrainCards.Count != 0 || claimRoutes.Count != 0)
                    {
                        AllPossibleMovesCount++;
                    }
                }
            }
        }

        public List<Move> GetAllPossibleMoves()
        {
            var allMoves = new List<Move>();

            if (ChooseDestinationCardMoves.Count != 0)
            {
                foreach (var chooseDestinationCardMove in ChooseDestinationCardMoves)
                {
                    allMoves.Add(chooseDestinationCardMove);
                };

                return allMoves;
            }

            foreach(var drawTrainCardMove in DrawTrainCardMoves)
            {
                allMoves.Add(drawTrainCardMove);
            }

            foreach (var claimRouteMove in ClaimRouteMoves)
            {
                allMoves.Add(claimRouteMove);
            }

            if(DrawDestinationCardMove != null)
            {
                if(CanDestinationCardBeTheOnlyMove)
                {
                    allMoves.Add(DrawDestinationCardMove);
                }
                else if (DrawTrainCardMoves.Count != 0 || ClaimRouteMoves.Count != 0)
                {
                    allMoves.Add(DrawDestinationCardMove);
                }
            }

            return allMoves;
        }
    }
}
