import { PlayerColor, TrainColor } from "./GameObjects.js";
import { cities } from "./game.js";
import { GameState } from "./gameStates.js";

export function getPlayerColorFromNumber(colorNumber) {
    switch (colorNumber) {
        case 0:
            return PlayerColor.Red;
        case 1:
            return PlayerColor.Blue;
        case 2:
            return PlayerColor.Yellow;
        case 3:
            return PlayerColor.Green;
        default:
            return null;
    }
}

export function getTrainColorFromNumber(colorNumber) {
    switch (colorNumber) {
        case 0:
            return TrainColor.Red;
        case 1:
            return TrainColor.Orange;
        case 2:
            return TrainColor.Yellow;
        case 3:
            return TrainColor.Green;
        case 4:
            return TrainColor.Blue;
        case 5:
            return TrainColor.Purple;
        case 6:
            return TrainColor.Black;
        case 7:
            return TrainColor.White;
        case 8:
            return TrainColor.Locomotive;
        default:
            return null; // or any default color
    }
}

export function getCityFromNumber(cityNumber){
    return cities[cityNumber];
}

export function getGameStateFromNumber(gameStateNumber){
    switch (gameStateNumber) {
        case 0:
            return GameState.WaitingForPlayerMove;
        case 1:
            return GameState.DrawingTrainCards;
        case 2:
            return GameState.DecidingAction;
        case 3:
            return GameState.ChoosingDestinationCards;
        default:
            return null;
    }
}