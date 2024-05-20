import { TrainCard, initPlayerHand } from "./GameObjects.js";
import { selectedCities } from "./board.js";
import { faceUpDeck, gameState, hideMessage, playerIndex, playerTurn, players, showMessage, updateGameState } from "./game.js";
import { GameState } from "./gameStates.js";
import { getTrainColorFromNumber } from "./getObjectsFromEnum.js";
import { emptyHtmlContainer } from "./utils.js";

let trainCardImages = {};
let canvasWidth = 400;
let canvasHeight = 600;
let spacing = 20;
let trainCanvas;

const imagePaths = {
    White: './images/traincards/white.png',
    Black: './images/traincards/black.png',
    Blue: './images/traincards/blue.png',
    Green: './images/traincards/green.png',
    Locomotive: './images/traincards/locomotive.png',
    Orange: './images/traincards/orange.png',
    Purple: './images/traincards/pink.png',
    Red: './images/traincards/red.png',
    Yellow: './images/traincards/yellow.png',
    Deck: './images/traincards/decktop.png'
};

export function preloadTrainCardImages() {
    if (Object.keys(trainCardImages).length === 0) {
        // Iterate over each image path and load the corresponding image
        for (const [key, path] of Object.entries(imagePaths)) {
            trainCardImages[key] = new Image();
            trainCardImages[key].src = path;
        }
    }
}


export function displayFaceUpDeckImages(images) {
    let container = emptyHtmlContainer('train-card-deck-container');
    loadDeckImage(container);

    for (const img of images) {
        // Create an image element
        let imageElement = document.createElement('img');
        // Assign a class to the image element
        imageElement.classList.add('train-card-image');
        imageElement.classList.add('face-up-deck-image');

        // Set the src attribute of the image element
        imageElement.src = imagePaths[getTrainColorFromNumber(img.color)]

        // Append the image element to the container
        container.appendChild(imageElement);

        if (!img.isAvailable) {
            imageElement.classList.add('face-up-deck-image__unavailable');
            continue;
        }

        imageElement.onclick = async function () {
            // Find the index of the clicked image
            let images = Array.from(container.children);
            let clickedIndex = images.indexOf(imageElement) - 1;
            await drawTrainCard(clickedIndex);
        };
    }
}

export function displayPlayerTrainCards(trainCards) {
    let colorCounts = {};

    let container = emptyHtmlContainer('player-train-cards-container');
    let canBeClicked =
        (gameState === GameState.WaitingForPlayerMove || gameState === GameState.DecidingAction)
        && playerIndex == playerTurn;

    for (const card of trainCards) {
        let color = getTrainColorFromNumber(card.color);

        // Increment the count for the current color
        colorCounts[color] = (colorCounts[color] || 0) + 1;

        // Check if there's already an image element with the same color class
        let existingImage = container.querySelector(`.player-train-card-image.${color}`);
        if (existingImage) {
            // Increment the count for the existing image
            let count = parseInt(existingImage.dataset.count || 0) + 1;
            existingImage.dataset.count = count;
            existingImage.alt = `${count} ${color} train cards`; // Update the alt text
            // Update the count displayed on top of the image
            existingImage.parentNode.querySelector('.card-count').textContent = count;
        } else {
            // Create a new container for the image and count
            let cardContainer = document.createElement('div');
            cardContainer.classList.add('train-card-container');

            // Create a new image element
            let imageElement = document.createElement('img');
            imageElement.classList.add('player-train-card-image', 'train-card-image', color);
            imageElement.src = imagePaths[color];
            imageElement.dataset.count = 1; // Set the initial count
            imageElement.alt = `1 ${color} train card`; // Set the alt text
            cardContainer.appendChild(imageElement);

            // Create a new element to display the count
            let countElement = document.createElement('div');
            countElement.classList.add('card-count');
            countElement.textContent = '1';
            cardContainer.appendChild(countElement);

            // Append the container to the main container
            container.appendChild(cardContainer);
            if (canBeClicked) {
                console.log("adding event listener")
                imageElement.addEventListener("click", async function () {
                    await claimRouteRequest(color);
                })
            }
        }

    }
}

async function drawTrainCard(faceUpCardIndex = -1) {
    if (gameState !== GameState.DrawingTrainCards
        && gameState !== GameState.WaitingForPlayerMove
        && gameState !== GameState.DecidingAction) {
        return;
    }

    hideMessage(false);
    hideMessage();

    let url = `http://localhost:5001/game/DrawTrainCard?playerIndex=${playerIndex}`;

    if (faceUpCardIndex !== -1) {
        url += `&faceUpCardIndex=${faceUpCardIndex}`
    }
    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });

    if (response.ok) {
        let responseJson = await response.json();
        console.log(responseJson);
        reloadFaceUpDeck(responseJson.faceUpDeck);
        reloadPlayerTrainCards(responseJson.playerHand, playerIndex);
    }
    else {
        let errorResponse = await response.text();
        console.error(errorResponse);
        showMessage(errorResponse);
    }
    await updateGameState();
}

function reloadPlayerTrainCards(trainCards, playerIndex) {
    let container = emptyHtmlContainer('player-train-cards-container');
    updatePlayerHand(trainCards, playerIndex);
    displayPlayerTrainCards(players[playerIndex].hand);
}

function reloadFaceUpDeck(faceUpDeckCards) {
    console.log(faceUpDeckCards)
    let container = emptyHtmlContainer('train-card-deck-container');
    updateFaceUpDeck(faceUpDeckCards);
    loadDeckImage(container);
    displayFaceUpDeckImages(faceUpDeck);
}

function updateFaceUpDeck(faceUpDeckCards) {
    faceUpDeck.length = 0

    for (const card of faceUpDeckCards) {
        faceUpDeck.push(new TrainCard(card.color, card.isAvailable))
    }
}

function updatePlayerHand(trainCards, playerIndex) {
    let playerHand = initPlayerHand(trainCards);
    players[playerIndex].hand = playerHand;
}

function loadDeckImage(container) {
    let imageElement = document.createElement('img');
    imageElement.classList.add('train-deck-image');
    imageElement.classList.add('train-card-image');
    imageElement.src = imagePaths['Deck']
    container.appendChild(imageElement);

    imageElement.onclick = async function () {
        await drawTrainCard();
    };
}

export async function chooseTrainCardColor() {
    gameState = GameState.DecidingAction;
}

export function buildColorsWithWhichRouteCanBeClaimedMessage(trainColors) {
    let message = `You can claim this route with the colors: `;
    console.log(trainColors)
    for (let trainColor of trainColors) {
        trainColor = getTrainColorFromNumber(trainColor);
        console.log(trainColor)
        message = message.concat(`${trainColor} `)
    }

    return message;
}

export async function claimRouteRequest(color) {
    let requestBody = {
        playerIndex: playerIndex,
        colorUsed: color,
        originCity: selectedCities[0].name,
        destinationCity: selectedCities[1].name
    };

    let url = `http://localhost:5001/game/ClaimRoute`;

    try {
        console.log(selectedCities.length)
        if (selectedCities.length !== 2) {
            return;
        }
        console.log(color)
        let response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        if (response.ok) {
            let responseJson = await response.json();
            hideMessage(false);
            await updateGameState();
            console.log(responseJson);
        }
        else {
            let errorResponse = await response.text();
            console.error(errorResponse);
            showMessage(errorResponse);
        }
    }
    catch (error) {
        console.error(error);
        showMessage(error);
    }
}
