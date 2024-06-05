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
        case 1:
            return TrainColor.Red;
        case 2:
            return TrainColor.Orange;
        case 3:
            return TrainColor.Yellow;
        case 4:
            return TrainColor.Green;
        case 5:
            return TrainColor.Blue;
        case 6:
            return TrainColor.Purple;
        case 7:
            return TrainColor.Black;
        case 8:
            return TrainColor.White;
        case 9:
            return TrainColor.Locomotive;
        default:
            return null; // or any default color
    }
}

export function getCityFromNumber(cityNumber) {
    return cities[cityNumber];
}

export function getCityIndexFromName(cityName) {
    for (const [index, city] of Object.entries(cities)) {
        if (city.name === cityName) {
            return parseInt(index);
        }
    }
    return null; // Return null if the city is not found
}

export function getGameStateFromNumber(gameStateNumber) {
    switch (gameStateNumber) {
        case 0:
            return GameState.WaitingForPlayerMove;
        case 1:
            return GameState.DrawingTrainCards;
        case 2:
            return GameState.DecidingAction;
        case 3:
            return GameState.ChoosingDestinationCards;
        case 4:
            return GameState.Ended;
        case 5:
            return GameState.DrawingFirstDestinationCards;
        case 6:
            return GameState.ChoosingFirstDestinationCards;
        default:
            return null;
    }
}