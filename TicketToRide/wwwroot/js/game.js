import { DestinationCard, TrainCard, Player, Route } from "./GameObjects.js";
import { drawBoard, preloadBoard, setupBoard } from "./board.js";
import { initializeCities } from "./city.js";
import { displayPlayerDestinationCards } from "./destinationCards.js";
import { GameState } from "./gameStates.js";
import { initDisplayPlayerStatistics } from "./playerStatistics.js";
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

export let playerIndex = getPlayerIndex();

const playerStatsContainer = document.getElementById('all-player-stats-container');
const createGameButton = document.getElementById("createGameButton");
const createGameButtonContainer = document.getElementById('init-game-button-container');
const newGameButton = document.getElementById('newGameButton');

preInitGame();

async function preInitGame() {
    let isGameLoaded = false;

    let url = `http://localhost:5001/game/IsGameInitialized`;
    let result = await fetch(url);

    if (result.ok) {
        isGameLoaded = await result.json();
        if (isGameLoaded) {
            //make request to display existent game
            changeVisibilities(false);
            displayGame();
        }
        else {
            //make request to get new game
            changeVisibilities(true);
            initGame();
        }
    }
    else {
        console.log(`error : ${result}`);
    }

}

async function initGame() {
    createGameButton.addEventListener("click", async function () {
        const numberOfPlayers = document.getElementById("numberOfPlayers").value;
        await displayGame(false, numberOfPlayers);
    });

    newGameButton.addEventListener("click", async function () {
        //make request to delete game
        await deleteGame();
    })
}

function changeVisibilities(loadingNewGame) {
    //when loading new game, show create game button, hide player stats 
    //and new game button
    console.log('here')
    playerStatsContainer.style.display = loadingNewGame == true ? 'none' : 'inline';
    createGameButtonContainer.style.display = loadingNewGame == true ? 'inline' : 'none';
    newGameButton.style.display = loadingNewGame == true ? 'none' : 'flex';
}

async function displayGame(exists = true, numberOfPlayers = 2) {
    let url;

    if (exists) {
        url = `http://localhost:5001/game`;
    }
    else {
        url = `http://localhost:5001/game/newGame?numberOfPlayers=${numberOfPlayers}`;
    }

    var response = await fetch(url);

    if (response.ok) {
        let responseJson = await response.json();
        initGameVariables(responseJson);
        initFaceUpDeck(faceUpDeck);
        initPlayerCardDeck(playerIndex)
        initPlayerDestinationCards(playerIndex);
        initPlayerStatistics(playerIndex);
        if (!exists) {
            changeVisibilities(false)
        }
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

function initFaceUpDeck() {
    preloadTrainCardImages();
    displayFaceUpDeckImages(faceUpDeck);
}

function initPlayerCardDeck(playerIndex) {
    displayPlayerTrainCards(players[playerIndex].hand)
}

function initPlayerDestinationCards(playerIndex) {
    displayPlayerDestinationCards(players[playerIndex]);
}

function initPlayerStatistics(playerIndex) {
    initDisplayPlayerStatistics(players, playerIndex);
}

function printGame() {
    console.log(gameInstance)
}

function getPlayerIndex() {
    const queryString = window.location.search;
    const params = new URLSearchParams(queryString);
    const playerIndex = params.get("playerIndex");
    if (playerIndex) {
        return playerIndex;
    }
    else {
        return 0;
    }
}

async function deleteGame(){
    let url = `http://localhost:5001/game`;

    var response = await fetch(url, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        },
    });
    if (response.ok) {
        window.location.reload();
    }
    else {
        console.error(response)
    }
}

window.preload = function () {
    preloadBoard();
}

window.setup = function () {
    setupBoard()
}

window.draw = function () {
    drawBoard();
}