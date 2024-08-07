import { DestinationCard, TrainCard, Player, Route } from "./GameObjects.js";
import { drawBoard, preloadBoard, setupBoard } from "./board.js";
import { initializeCities } from "./city.js";
import { getGameOutcomeRequest } from "./computeGameOutcomeRequest.js";
import { openCreateNewGameSweetAlert } from "./createGameSweetAlert.js";
import { displayPlayerDestinationCards, emptyHoveredCityNames, hoveredCityNames } from "./destinationCards.js";
import { displayGameLog } from "./gameLog.js";
import { GameState } from "./gameStates.js";
import { getExistingGameRequest, getNewGameRequest } from "./getGameRequest.js";
import { getGameStateRequest } from "./getGameStateRequest.js";
import { getCityFromNumber, getGameStateFromNumber } from "./getObjectsFromEnum.js";
import { makeBotMoveRequest } from "./makeBotMoveRequest.js";
import { makeNextMoveRequest } from "./makeNextMoveRequest.js";
import { initDisplayPlayerStatistics } from "./playerStatistics.js";
import { displayFaceUpDeckImages, displayPlayerTrainCards, preloadTrainCardImages } from "./trainCardsDeck.js";
import { delay, emptyHtmlContainer } from "./utils.js";

let isGameAReplay = false;
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

await shouldReplayGame();

export let currentPlayer;

export let hasGameEnded = false;

let displayingGameResults = false;
let hasMadeRequest = false;
let updateGameStateInProgress = false;
let intervalId; // To keep track of the interval ID


let numberOfPlayers;
let playerTypes = [];

const newGameButton = document.getElementById('newGameButton');
const drawDestinationCardsButton = document.getElementById('drawDestinationCardsButton');
const chooseDestinationsButton = document.getElementById('chooseDestinationsButton');
const mainContainer = document.getElementById('main-container');


window.preload = function () {
    preloadBoard();
}

window.setup = function () {
    setupBoard();
}

window.draw = function () {
    drawBoard();
}

console.log('yes')
addButtonEventListeners();
preInitGame();

async function shouldReplayGame() {
    let guid = getGUID();
    if (guid) {
        //should replay game
        isGameAReplay = true;
        let url = `http://localhost:5001/game/LoadGameForReplay?guid=${guid}`;
        let result = await fetch(url);
        if (!result.ok) {
            let error = await result.text();
            console.error(error);
        }
        setTimeout(() => {}, 1000);
    }
}

async function preInitGame() {
    setTimeout(() => {}, 3000);
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
            //open swal

            const result = await openCreateNewGameSweetAlert();
            if (result) {
                numberOfPlayers = result.numberOfPlayers;
                playerTypes = result.playerTypes;
                displayGame(false)
            }
        }
    }
    else {
        console.log(`error : ${result}`);
    }
}

async function addButtonEventListeners() {
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
    mainContainer.style.display = loadingNewGame == true ? 'none' : 'flex';
}

async function displayGame(exists = true) {
    let response;

    if (exists) {
        response = await getExistingGameRequest(playerIndex);
    }
    else {
        response = await getNewGameRequest(numberOfPlayers, playerTypes);
    }

    if (response.ok) {
        let responseJson = await response.json();
        
        initGameVariables(responseJson);
        initFaceUpDeck(faceUpDeck);
        initPlayerCardDeck(playerIndex)
        initPlayerStatistics(playerIndex);
        initMessagesContainer(playerTurn, playerIndex);
        initGameLog(responseJson.gameLog)
        await initPlayerDestinationCards(playerIndex);
        console.log("finish others")
        if (!intervalId) {
            intervalId = setInterval(async () => {
                if (!updateGameStateInProgress) {
                    updateGameStateInProgress = true;
                    await updateGameState();
                    updateGameStateInProgress = false;
                }
            }, 100);
        }

        if (!exists) {
            changeVisibilities(false)
        }
    }
    else {
        await displayNothing();
    }
}

async function displayNothing() {
    let container = document.getElementById(`main-container`);
    container.innerHTML = '';
    const result = await Swal.fire('Game is not available for this player index');
}

function initGameVariables(game) {
    playerTurn = game.playerTurn;
    gameState = getGameStateFromNumber(game.gameState);
    trainCardsDeck = [];
    faceUpDeck = [];
    destinationCardsDeck = [];
    players = [];
    routes = [];
    isGameAReplay = game.isGameAReplay;

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
            player.playerIndex,
            player.pendingDestinationCards,
            player.hand,
            player.completedDestinationCards,
            player.numberOfTrainCards,
            player.numberOfPendingDestinationCards,
            player.numberOfCompletedDestinationCards,
            player.claimedRoutes,
            player.isBot
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

    hasGameEnded = gameState == GameState.Ended;

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
        claimedRoutes: claimedRoutes,
        hasGameEnded: hasGameEnded
    }

    currentPlayer = players[playerIndex];

    printGame();
}

function initFaceUpDeck() {
    preloadTrainCardImages();
    displayFaceUpDeckImages(faceUpDeck);
}

function initPlayerCardDeck(playerIndex) {
    displayPlayerTrainCards(players[playerIndex].hand)
}

async function initPlayerDestinationCards(playerIndex) {
    if (players[playerIndex].isBot == false
        && gameState == GameState.DrawingFirstDestinationCards
        && (players[playerIndex].pendingDestinationCards.length == 0
            || players[playerIndex].pendingDestinationCards == null)) {
        await drawDestinationCardsRequest();
        displayPlayerDestinationCards(players[playerIndex]);
    }
    else {
        displayPlayerDestinationCards(players[playerIndex]);
    }
}

function initPlayerStatistics(playerIndex) {
    initDisplayPlayerStatistics(players, playerIndex);
}

function initGameLog(gameLog) {
    displayGameLog(gameLog);
}

export function initMessagesContainer(playerTurn, playerIndex) {
    var playerTurnContainer = document.getElementById('messages-container__player-turn-message');
    var playerWaitingContainer = document.getElementById('messages-container__waiting-message');
    var gameEndedContainer = document.getElementById('messages-container__game-ended-message');
    if (hasGameEnded) {
        gameEndedContainer.style.display = 'inline';
        playerTurnContainer.style.display = 'none';
        playerWaitingContainer.style.display = 'none';
        return;
    }

    if (playerTurn != playerIndex) {
        playerTurnContainer.style.display = 'none';
        playerWaitingContainer.style.display = 'inline';
    }
    else {
        playerWaitingContainer.style.display = 'none';
        if (gameState !== GameState.ChoosingDestinationCards
            && gameState !== GameState.ChoosingFirstDestinationCards
        ) {
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

function getGUID() {
    const queryString = window.location.search;
    const params = new URLSearchParams(queryString);
    const guid = params.get("guid");
    if (guid) {
        return guid;
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


    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
    });

    if (response.ok) {
        let responseJson = await response.json();
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
        displayDestinationCards(responseJson.drawnDestinationCards);
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
        if (isGameAReplay && !hasGameEnded && playerIndex == 0) {
            updateReplayGameState();
        }
        else if (!hasGameEnded) {
            let updatedGameState = await getGameStateRequest();
            let oldGameState = gameState;
            let oldPlayerTurn = playerTurn;

            playerTurn = updatedGameState.playerTurn;
            gameState = getGameStateFromNumber(updatedGameState.gameState);
            hasGameEnded = gameState == GameState.Ended;

            if (playerTurn == playerIndex
                && currentPlayer.isBot
                && hasMadeRequest == false
                && !isGameAReplay) {
                //get next move if the game is not ended and it's the bot's turn
                hasMadeRequest = true;
                await makeBotMoveRequest(playerIndex);
                hasMadeRequest = false;
                updateGameState();
            }

            if (oldGameState !== gameState || playerTurn !== oldPlayerTurn) {
                displayGame(true, numberOfPlayers)
            }
        }
        else {
            if (displayingGameResults == false) {
                let gameOutcome = await getGameOutcomeRequest();
                displayWinners(gameOutcome);
            }
        }
    }
    catch (error) {
        console.error(error)
    }
}

async function updateReplayGameState() {
    if (isGameAReplay && playerIndex == 0) {
        let game = await makeNextMoveRequest();
        let updatedGameState = await getGameStateRequest();

        gameState = getGameStateFromNumber(updatedGameState.gameState);
        if (gameState == GameState.Ended) {
            hasGameEnded = true;
        }
        displayGame(true, numberOfPlayers);
    }
}

export function showMessage(message, isError = true, setTimout = true) {
    //hide error or information container
    hideMessage(!isError);
    let container;
    let messageSpan;

    if (isError) {
        container = document.getElementById('messages-container__error-message');
        let copyMessage = message;
        message = `Error: ${copyMessage}`;
    }
    else {
        container = document.getElementById('messages-container__information-message');
    }

    messageSpan = container.querySelector('span')
    messageSpan.innerHTML = message;
    container.style.display = "block";

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

    var messageSpan = span == null ? container.querySelector('span') : span;

    messageSpan.innerHTML = '';
    container.style.display = "none";
}

function displayWinners(gameOutcome) {
    let winners = gameOutcome.winners;
    let players = gameOutcome.players;
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
            winner.playerIndex,
            winner.pendingDestinationCards,
            winner.hand,
            winner.completedDestinationCards,
            winner.numberOfTrainCards,
            winner.numberOfPendingDestinationCards,
            winner.numberOfCompletedDestinationCards,
            winner.claimedRoutes,
            winner.isBot
        ))
    }

    //if there is one winner
    if (winners.length === 1) {
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
    htmlContent += `<div>${message}</div>`
    // Concatenate player names and points
    players.forEach((player) => {
        htmlContent += `<div>${player.name} : ${player.points} p</div>`;
    });

    htmlContent += `<div>${players[gameOutcome.longestContPathPlayerIndex].name} has the longest path : ${gameOutcome.longestContPathLength} trains </div>`;

    htmlContent += '</div>';

    // Display message using SweetAlert
    Swal.fire({
        title: "Game Over",
        html: htmlContent,
    }).then((result) => {
        if (result.isConfirmed) {
            displayingGameResults = true;
            document.getElementById("player-points").textContent = playerPoints;
        }
    });
}

export function setGameState(state) {
    gameState = state;
}