import { getTrainColorFromNumber } from "./getObjectsFromEnum.js";

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
    // Iterate over each image path and load the corresponding image
    for (const [key, path] of Object.entries(imagePaths)) {
        trainCardImages[key] = new Image();
        trainCardImages[key].src = path;
    }

    //add deck card
    let container = document.getElementById('train-card-deck-container');
    let imageElement = document.createElement('img');
    imageElement.classList.add('train-deck-image');
    imageElement.classList.add('train-card-image');
    imageElement.src = imagePaths['Deck']
    container.appendChild(imageElement);
}


export function displayFaceUpDeckImages(images) {
    console.log(images);
    let container = document.getElementById('train-card-deck-container');
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
    }
}

export function displayPlayerTrainCards(trainCards){
    let colorCounts = {};

    let container = document.getElementById('player-train-cards-container');
    for (const card of trainCards) {
        let color = getTrainColorFromNumber(card.color);
        
        // Increment the count for the current color
        colorCounts[color] = (colorCounts[color] || 0) + 1;

        // Check if there's already an image element with the same color class
        let existingImage = container.querySelector(`.player-train-card-image.${color}`);
        console.log(colorCounts)
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
        }
    }
}
