import { PlayerColor, TrainColor } from "./GameObjects.js";
import { City, initializeCities } from "./city.js";
import { cities } from "./game.js";
import { getPlayerColorFromNumber } from "./getObjectsFromEnum.js";

let boardImg; // Variable to store the board image
let canvasWidth, canvasHeight; // Variables to store canvas dimensions
let boardCanvas;
let selectedCities = []
let claimedRoutes = []
let claimedRoute = {
    origin: 2,
    destination: 35,
    color: TrainColor.White,
    length: 6,
    isClaimed: true,
    claimedBy: 1
}
claimedRoutes.push(claimedRoute)

export function preloadBoard() {
    // Load the board image
    boardImg = loadImage('./images/board.jpg');
    //train images = trainCardsDeckJs.load images
}

export function setupBoard() {
    // Calculate initial canvas width and height
    canvasWidth = windowWidth / 2;
    canvasHeight = windowHeight / 1.5;

    // Create a canvas with calculated width and height
    boardCanvas = createCanvas(canvasWidth, canvasHeight);
    boardCanvas.parent('board-container')

    // Position the canvas using CSS styles
    // boardCanvas.position(0, 0); // Position relative to the top-left corner of the window
    // boardCanvas.style('left', '100px');
    // boardCanvas.style('top', '100px');

    boardCanvas.mouseClicked(handleClick);
}

export function drawBoard() {
    // Draw the board image on the canvas
    if (boardImg) {
        background(255); // Clear the background
        image(boardImg, 0, 0, width, height); // Draw the board image
    }
    for (const city of cities) {
        drawCityCircle(city, selectedCities.includes(city));
    }
    for (const route of claimedRoutes) {
        drawClaimedRoute(route);
    }

}

window.windowResized = function () {
    // Recalculate canvas width and height
    canvasWidth = windowWidth / 2;
    canvasHeight = windowHeight / 1.5;

    // Resize the canvas
    resizeCanvas(canvasWidth, canvasHeight);

    boardCanvas.position(0, 0); // Position relative to the top-left corner of the window
    boardCanvas.style('left', '100px');
    boardCanvas.style('top', '100px');
}

function handleClick() {

    let x = mouseX
    let y = mouseY
    // Output the coordinates
    console.log("Clicked at: (" + x + ", " + y + ")");
    drawCityCircle(x, y)
}

function drawCityCircle(city, isSelected = false) {
    let d = dist(mouseX, mouseY, city.x, city.y);
    // If the mouse is over the ellipse, change its color
    if (isSelected) {
        fill(0, 255, 0); // Change color to green
    }
    else if (d < 5) { // Adjust the radius (5) as needed
        fill(255, 0, 0); // Change color to red
    } else {
        fill(255, 255, 255); // Default color
    }

    ellipse(city.x, city.y, 10, 10);
}

function drawClaimedRoute(route) {
    let originCity = cities[route.origin]
    let destinationCity = cities[route.destination]

    let point1 = createVector(originCity.x, originCity.y);
    let point2 = createVector(destinationCity.x, destinationCity.y);

    changeStroke(route.claimedBy)

    // Draw the line between the points
    line(point1.x, point1.y, point2.x, point2.y);

    resetStroke();

}

document.addEventListener('click', function (event) {
    async function handleClick(city) {
        console.log("Clicked on city:", city.name);
        await changeSelectedCities(city);
    }

    // Check if the click is on any city
    cities.forEach(city => {
        let d = dist(mouseX, mouseY, city.x, city.y);
        if (d < 5) { // Adjust the radius (5) as needed
            handleClick(city)
        }
    });
});

async function changeSelectedCities(city) {
    if (selectedCities.includes(city)) {
        selectedCities = selectedCities.filter(selectedCity => selectedCity !== city);
        return;
    }

    if (selectedCities.length == 2) {
        selectedCities = []
    }

    selectedCities.push(city);

    if (selectedCities.length == 2) {
        await checkIfRouteExists(selectedCities);
    }

    console.log(selectedCities)
}

async function checkIfRouteExists(selectedCities) {
    let origin = selectedCities[0].name;
    let destination = selectedCities[1].name;

    let url = `http://localhost:5001/game/DoesRouteExist?origin=${origin}&&destination=${destination}`;

    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });
    if (response.ok) {
        let responseJson = await response.json();
        console.log(responseJson)
    }
    else {
        console.error(response)
    }
}

function changeStroke(playerColor) {
    strokeWeight(4);

    let color = getPlayerColorFromNumber(playerColor);
    switch (color) {
        case PlayerColor.Blue:
            stroke(0, 0, 255);
            return;
        case PlayerColor.Red:
            stroke(255, 0, 0);
            return;
        case PlayerColor.Green:
            stroke(0, 255, 0);
            return;
        case PlayerColor.Yellow:
            stroke(0, 255, 255);
            return;
        default:
            stroke(0, 0, 0);
    }

}
function resetStroke() {
    strokeWeight(1);
    stroke(0, 0, 0);
}