import { DestinationCard, TrainCard, Player, Route } from "./GameObjects.js";
import { drawBoard, preloadBoard, setupBoard } from "./board.js";
import { initializeCities } from "./city.js";
import { computeGameOutcomeRequest } from "./computeGameOutcomeRequest.js";
import { displayPlayerDestinationCards, emptyHoveredCityNames, hoveredCityNames } from "./destinationCards.js";
import { GameState } from "./gameStates.js";
import { getGameStateRequest } from "./getGameStateRequest.js";
import { getCityFromNumber, getCityIndexFromName, getGameStateFromNumber } from "./getObjectsFromEnum.js";
import { initDisplayPlayerStatistics } from "./playerStatistics.js";
import { chooseTrainCardColor, displayFaceUpDeckImages, displayPlayerTrainCards, preloadTrainCardImages } from "./trainCardsDeck.js";
import { emptyHtmlContainer } from "./utils.js";

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
let displayingGameResults = false;

const playerStatsContainer = document.getElementById('all-player-stats-container');
const messagesContainer = document.getElementById('messages-container');
const createGameButton = document.getElementById("createGameButton");
const createGameButtonContainer = document.getElementById('init-game-button-container');
const newGameButton = document.getElementById('newGameButton');
const drawDestinationCardsButton = document.getElementById('drawDestinationCardsButton');
const chooseDestinationsButton = document.getElementById('chooseDestinationsButton');

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
        await drawDestinationCardsRequest();
    });

    chooseDestinationsButton.addEventListener("click", async function () {
        const destinations = [];

        document.querySelectorAll('#possible-destinations-list div').forEach(div => {
            const checkbox = div.querySelector('input[type="checkbox"]');
            const [originCity, destinationCity] = checkbox.value.split('-');
            destinations.push({
                origin: originCity,
                destination: destinationCity,
                isChosen: checkbox.checked
            });
        });
        await chooseDestinationsRequest(destinations);
    })
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
        console.log(`response`);
        console.log(responseJson)
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
            player.completedDestinationCards,
            player.numberOfTrainCards,
            player.numberOfPendingDestinationCards,
            player.numberOfCompletedDestinationCards,
            player.claimedRoutes
        ));
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
        playerWaitingContainer.style.display = 'none';
        if (gameState !== GameState.ChoosingDestinationCards) {
            playerTurnContainer.style.display = 'flex';
        }
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

async function chooseDestinationsRequest(destinations) {
    let url = `http://localhost:5001/game/DrawDestinationCards`;

    let requestBody = {
        playerIndex: playerIndex,
        destinationCards: destinations
    };

    console.log(requestBody)

    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
    });

    if (response.ok) {
        let responseJson = await response.json();
        console.log("chosen dest", responseJson);
        hideDestinationCardsChoices();
        await updateGameState();
    }
    else {
        let errorResponse = await response.text();
        console.error(errorResponse);
        showMessage(errorResponse);
    }
}

async function drawDestinationCardsRequest() {
    let url = `http://localhost:5001/game/DrawDestinationCards?playerIndex=${playerIndex}`;

    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });
    if (response.ok) {
        const responseJson = await response.json();
        console.log(responseJson)
        displayDestinationCards(responseJson.drawnDestinationCards);
        console.log(responseJson)
    }
    else {
        const errorText = await response.text();
        console.error(errorText);
    }
}

function displayDestinationCards(destinations) {
    let chooseDestinationCardsContainer = document.getElementById('choose-destination-cards-container');
    chooseDestinationCardsContainer.style.display = 'flex';

    let playerTurnContainer = document.getElementById('messages-container__player-turn-message');
    playerTurnContainer.style.display = 'none';

    let destinationsList = document.getElementById('possible-destinations-list');;

    destinations.forEach(destination => {
        const originCity = getCityFromNumber(destination.origin);
        const destinationCity = getCityFromNumber(destination.destination);

        const div = document.createElement('div');
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.value = `${originCity.name}-${destinationCity.name}`;

        const span = document.createElement('span');
        span.textContent = `${originCity.name} - ${destinationCity.name} ${destination.pointValue} p`;

        span.addEventListener('mouseover', () => {
            span.classList.add('hovered-destination-card');
            hoveredCityNames.push(originCity.name);
            hoveredCityNames.push(destinationCity.name);
        });

        span.addEventListener('mouseout', () => {
            span.classList.remove('hovered-destination-card');
            emptyHoveredCityNames();
        });

        checkbox.addEventListener('change', checkCheckboxes);

        div.appendChild(checkbox);
        div.appendChild(span);
        destinationsList.appendChild(div);
    });
}

function hideDestinationCardsChoices() {
    let chooseDestinationCardsContainer = document.getElementById('choose-destination-cards-container');
    chooseDestinationCardsContainer.style.display = 'none';

    emptyHtmlContainer('possible-destinations-list');
}

function checkCheckboxes() {
    const checkboxes = document.querySelectorAll('#possible-destinations-list input[type="checkbox"]');
    const atLeastOneChecked = Array.from(checkboxes).some(checkbox => checkbox.checked);
    chooseDestinationsButton.disabled = !atLeastOneChecked;
}

export async function updateGameState() {
    try {
        let updatedGameState = await getGameStateRequest();
        let oldGameState = gameState;
        let oldPlayerTurn = playerTurn;

        playerTurn = updatedGameState.playerTurn;
        gameState = getGameStateFromNumber(updatedGameState.gameState);
        if (gameState == GameState.Ended && displayingGameResults == false) {
            //make request to add up final points and show pop up with finished result and winner
            let gameOutcome = await computeGameOutcomeRequest();
            displayWinners(gameOutcome.winners);
        }
        if (oldGameState !== gameState || playerTurn !== oldPlayerTurn) {
            displayGame(true, numberOfPlayers)
        }
    }
    catch (error) {
        console.error(error)
    }
}

export function showMessage(message, isError = true, setTimout = true) {
    //hide error or information container
    hideMessage(!isError);
    let container;
    let messageSpan;

    if (isError) {
        container = document.getElementById('messages-container__error-message');
    }
    else {
        container = document.getElementById('messages-container__information-message');
    }

    messageSpan = container.querySelector('span')
    messageSpan.innerHTML = message;
    container.style.display = "inline";

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

function displayWinners(winners) {
    displayingGameResults = true;
    const currentPlayer = players[playerIndex];
    let winnerPlayers = [];
    let message = '';
    let playerPoints = '';

    for (const winner of winners) {
        winnerPlayers.push(new Player(
            winner.name,
            winner.points,
            winner.remainingTrains,
            winner.color,
            winner.pendingDestinationCards,
            winner.hand,
            winner.completedDestinationCards,
            winner.numberOfTrainCards,
            winner.numberOfPendingDestinationCards,
            winner.numberOfCompletedDestinationCards,
            winner.claimedRoutes
        ))
    }

    //if there is one winner
    if (winners.length === 1) {
        console.log(winnerPlayers)
        console.log(currentPlayer)
        if (winnerPlayers[0].name == currentPlayer.name) {
            message = 'You Won!';
        }
        else {
            message = `You Lost :( The winner is ${winnerPlayers[0].name}.`;
        }
    }
    else {
        //there are more winners: there is a draw
        if (winnerPlayers.includes(currentPlayer)) {
            message = 'You are one of the winners! ';
        }
        message += 'The winners are: ';
        for (const winner of winnerPlayers) {
            message += `${winner.name} - ${winner.points} p\n`;
            playerPoints += `${winner.name} - ${winner.points} p\n`;
        }
    }

    let htmlContent = '<div>';
    htmlContent +=`<div>${message}</div>`
    // Concatenate player names and points
    players.forEach((player) => {
        htmlContent += `<div>${player.name} : ${player.points} p</div>`;
    });

    htmlContent += '</div>';

    // Display message using SweetAlert
    Swal.fire({
        title: "Game Over",
        html: htmlContent,
    }).then((result) => {
        if (result.isConfirmed) {
            displayingGameResults = false;
            document.getElementById("player-points").textContent = playerPoints;
        }
    });
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