import { getCityFromNumber, getPlayerColorFromNumber, getTrainColorFromNumber } from "./getObjectsFromEnum.js";

export class TrainCard {
    constructor(color, isAvailable) {
        this.color = color;
        this.isAvailable = isAvailable;
    }
}

export class DestinationCard {
    constructor(origin, destination, pointValue) {
        this.origin = origin;
        this.destination = destination;
        this.pointValue = pointValue;
    }
}

export class Player {
    constructor(name, points, remainingTrains, color, pendingDestinationCards, hand, completedDestinationCards) {
        this.name = name;
        this.points = points;
        this.remainingTrains = remainingTrains;
        this.color = getPlayerColorFromNumber(color);
        this.pendingDestinationCards = initPlayerDestinationCards(pendingDestinationCards);
        this.completedDestinationCards = initPlayerDestinationCards(completedDestinationCards);
        this.hand = initPlayerHand(hand);
    }
}

export class Route{
    constructor(origin, destination, color, length, isClaimed, claimedBy, pointValue){
        this.origin = getCityFromNumber(origin);
        this.destination = getCityFromNumber(destination);
        this.color = getTrainColorFromNumber(color);
        this.length = length;
        this.isClaimed = isClaimed;
        this.claimedBy = this.claimedBy;
        this.pointValue = this.pointValue;
    }
}
export const TrainColor = {
    Red: 'Red',
    Orange: 'Orange',
    Yellow: 'Yellow',
    Green: 'Green',
    Blue: 'Blue',
    Purple: 'Purple',
    Black: 'Black',
    White: 'White',
    Locomotive: 'Locomotive'
}

export const PlayerColor = {
    Red: 'Red',
    Blue: 'Blue',
    Yellow: 'Yellow',
    Green: 'Green'
}


function initPlayerHand(hand){
    let playerHand = [];
    for(const card of hand){
        playerHand.push(new TrainCard(card.color,true))
    }
    return playerHand;
}

function initPlayerDestinationCards(destinationCards){
    let playerDestinationCards = [];
    for(const card of destinationCards){
        // let origin = getCityFromNumber(card.origin);
        // let destination = getCityFromNumber(card.destination)
        playerDestinationCards.push(new DestinationCard(card.origin, card.destination, card.pointValue))
    }
    return playerDestinationCards;
}
