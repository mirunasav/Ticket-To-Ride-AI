import { DestinationCard, TrainCard, Player, Route } from "./GameObjects.js";
import { drawBoard, preloadBoard, setupBoard } from "./board.js";
import { initializeCities } from "./city.js";
import { GameState } from "./gameStates.js";
import { displayFaceUpDeckImages, displayPlayerTrainCards, preloadTrainCardImages } from "./trainCardsDeck.js";

export let playerTurn = 0;
export let players = [];

export let trainCardsDeck = [];
export let destinationCardsDeck = [];
export let discardPile = [];
export let faceUpDeck = [];
export let cities = initializeCities();
export let gameState = GameState.WaitingForPlayerMove;
export let routes = [];

export let gameInstance = {};

export let playerIndex = document.getElementById('player-index').textContent;

initGame();

async function initGame(numberOfPlayers = 2) {
    let url = `http://localhost:5001/game/newgame?numberOfPlayers=${numberOfPlayers}`;

    var response = await fetch(url);
    if (response.ok) {
        let responseJson = await response.json();
        console.log(responseJson);
        initGameVariables(responseJson);
        initFaceUpDeck(faceUpDeck);
        initPlayerCardDeck(playerIndex)
    }
    else {
        console.error(response);
    }
}

function initGameVariables(game) {
    for (const card of game.board.deck) {
        trainCardsDeck.push(new TrainCard(card.color, true))
    }

    for (const card of game.board.faceUpDeck) {
        faceUpDeck.push(new TrainCard(card.color, true))
    }

    for (const card of game.board.destinationCards) {
        destinationCardsDeck.push(new DestinationCard(card.origin, card.destination, card.pointValue));
    }

    for (const player of game.players) {
        players.push(new Player(
            player.name,
            player.points,
            player.remainingTrains,
            player.color,
            player.pendingDestinationCards,
            player.hand,
            player.completedDestinationCards))
    }

    for (const route of game.board.routes.routes) {
        routes.push(new Route(
            route.origin,
            route.destination,
            route.color,
            route.length,
            route.isClaimed,
            route.claimedBy,
            route.pointValue))
    }

    gameInstance = {
        playerTurn: playerTurn,
        gameState: gameState,
        players: players,
        trainCardsDeck: trainCardsDeck,
        destinationCardsDeck: destinationCardsDeck,
        discardPile: discardPile,
        faceUpDeck: faceUpDeck,
        cities: cities,
        routes: routes,
        claimedRoutes: routes.filter(route => route.isClaimed)
    }

    printGame();
}

function initFaceUpDeck(){
    preloadTrainCardImages();
    displayFaceUpDeckImages(faceUpDeck);
}

function initPlayerCardDeck(playerIndex){
    displayPlayerTrainCards(players[playerIndex].hand)
}
function printGame() {
    console.log(gameInstance)
}

window.preload = function () {
    preloadBoard();
    //preLoadTrainCardImages();
}

window.setup = function () {
    // setupTrainCardDeck();
    setupBoard()
}

window.draw = function () {
    drawBoard();
    //  drawTrainCardDeck();
}