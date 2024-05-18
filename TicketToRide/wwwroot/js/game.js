import { DestinationCard, TrainCard, Player, Route } from "./GameObjects.js";
import { drawBoard, preloadBoard, setupBoard } from "./board.js";
import { initializeCities } from "./city.js";
import { displayPlayerDestinationCards } from "./destinationCards.js";
import { GameState } from "./gameStates.js";
import { getGameStateRequest } from "./getGameStateRequest.js";
import { getGameStateFromNumber } from "./getObjectsFromEnum.js";
import { initDisplayPlayerStatistics } from "./playerStatistics.js";
import { chooseTrainCardColor, displayFaceUpDeckImages, displayPlayerTrainCards, preloadTrainCardImages } from "./trainCardsDeck.js";

export let playerTurn;
export let players = [];

export let trainCardsDeck = [];
export let destinationCardsDeck = [];
export let discardPile = [];
export let faceUpDeck = [];
export let cities = initializeCities();
export let gameState;
export let routes = [];
export let claimedRoutes = [];

export let gameInstance = {};

export let playerIndex = getPlayerIndex();

const playerStatsContainer = document.getElementById('all-player-stats-container');
const messagesContainer = document.getElementById('messages-container');
const createGameButton = document.getElementById("createGameButton");
const createGameButtonContainer = document.getElementById('init-game-button-container');
const newGameButton = document.getElementById('newGameButton');
const drawDestinationCardsButton = document.getElementById('drawDestinationCardsButton');

addButtonEventListeners();
preInitGame();

setInterval(updateGameState, 3000);

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
        }
    }
    else {
        console.log(`error : ${result}`);
    }

}

async function addButtonEventListeners() {
    createGameButton.addEventListener("click", async function () {
        const numberOfPlayers = document.getElementById("numberOfPlayers").value;
        await displayGame(false, numberOfPlayers);
    });

    newGameButton.addEventListener("click", async function () {
        //make request to delete game
        await deleteGameRequest();
    });

    drawDestinationCardsButton.addEventListener("click", async function () {
        //make request to draw 2 cards, etc
    });
}

function changeVisibilities(loadingNewGame) {
    //when loading new game, show create game button, hide player stats 
    //and new game button
    hideMessage();//hide error messages
    hideMessage(false); // hide information messages
    playerStatsContainer.style.display = loadingNewGame == true ? 'none' : 'inline';
    createGameButtonContainer.style.display = loadingNewGame == true ? 'inline' : 'none';
    newGameButton.style.display = loadingNewGame == true ? 'none' : 'flex';
    messagesContainer.style.display = loadingNewGame == true ? 'none' : 'flex';
}

async function displayGame(exists = true, numberOfPlayers = 2) {
    let url;

    if (exists) {
        url = `http://localhost:5001/game?playerIndex=${playerIndex}`;
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
        initMessagesContainer(playerTurn, playerIndex);
        if (!exists) {
            changeVisibilities(false)
        }
    }
    else {
        console.error(response);
    }
}

function initGameVariables(game) {
    playerTurn = game.playerTurn;
    gameState = getGameStateFromNumber(game.gameState);
    trainCardsDeck = [];
    faceUpDeck = [];
    destinationCardsDeck = [];
    players = [];
    routes = [];

    console.log(game);
    for (const card of game.board.deck) {
        trainCardsDeck.push(new TrainCard(card.color, true))
    }

    for (const card of game.board.faceUpDeck) {
        faceUpDeck.push(new TrainCard(card.color, card.isAvailable))
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

    claimedRoutes = routes.filter(route => route.isClaimed);
    console.log(claimedRoutes)
    gameInstance = {
        playerTurn: playerTurn,
        gameState: getGameStateFromNumber(game.gameState),
        players: players,
        trainCardsDeck: trainCardsDeck,
        destinationCardsDeck: destinationCardsDeck,
        discardPile: discardPile,
        faceUpDeck: faceUpDeck,
        cities: cities,
        routes: routes,
        claimedRoutes: claimedRoutes
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

export function initMessagesContainer(playerTurn, playerIndex) {
    var playerTurnContainer = document.getElementById('messages-container__player-turn-message');
    var playerWaitingContainer = document.getElementById('messages-container__waiting-message');

    if (playerTurn != playerIndex) {
        playerTurnContainer.style.display = 'none';
        playerWaitingContainer.style.display = 'inline';
    }
    else {
        playerTurnContainer.style.display = 'flex';
        playerWaitingContainer.style.display = 'none';
    }
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

async function deleteGameRequest() {
    let url = `http://localhost:5001/game/delete`;

    console.log('deleting')
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

export async function updateGameState() {
    try {
        let updatedGameState = await getGameStateRequest();
        let oldGameState = gameState;
        let oldPlayerTurn = playerTurn;

        playerTurn = updatedGameState.playerTurn;
        gameState = getGameStateFromNumber(updatedGameState.gameState);
        if (oldGameState !== gameState || playerTurn !== oldPlayerTurn) {
            displayGame(true, numberOfPlayers)
        }
    }
    catch (error) {
        console.error(error)
    }
}

export function showMessage(message, isError = true, setTimout = true) {
    //hide message or information container
    hideMessage(!isError);
    let container;
    let messageSpan;

    if (isError) {
        container = document.getElementById('messages-container__error-message');
    }
    else {
        container = document.getElementById('messages-container__information-message');
    }

    console.log(message)
    messageSpan = container.querySelector('span')
    messageSpan.innerHTML = message;
    container.style.display = "inline";

    console.log(`msg span : ${messageSpan.innerHTML}`)

    if (setTimout) {
        // Fade out the message after 3 seconds
        setTimeout(() => {
            hideMessage(isError, container, messageSpan);
        }, 3000);
    }
}

export function hideMessage(isError = true, container = null, span = null) {
    if (container == null) {
        if (isError) {
            container = document.getElementById('messages-container__error-message');
        }
        else {
            container = document.getElementById('messages-container__information-message');
        }
    }
    console.log(container)
    var messageSpan = span == null ? container.querySelector('span') : span;

    messageSpan.innerHTML = '';
    container.style.display = "none";
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

export function setGameState(state) {
    gameState = state;
}