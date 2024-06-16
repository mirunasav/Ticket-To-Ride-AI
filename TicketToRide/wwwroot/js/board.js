import { PlayerColor, TrainColor } from "./GameObjects.js";
import { hoveredCityNames } from "./destinationCards.js";
import { cities, claimedRoutes, playerIndex, showMessage } from "./game.js";
import { buildColorsWithWhichRouteCanBeClaimedMessage } from "./trainCardsDeck.js";

let boardImg; // Variable to store the board image
let canvasWidth, canvasHeight; // Variables to store canvas dimensions
let boardCanvas;
export let selectedCities = [];

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
        drawCityCircle(city, selectedCities.includes(city), hoveredCityNames.includes(city.name));
    }
    for (const route of claimedRoutes) {
        console.log(claimedRoutes)
        let result = isDoubleRoute(route, claimedRoutes);
        drawClaimedRoute(route, result.isRouteDouble, result.isSecondRoute);
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

function drawCityCircle(city, isSelected = false, isDestinationHovered = false) {
    let d = dist(mouseX, mouseY, city.x, city.y);
    // If the mouse is over the ellipse, change its color
    if (isDestinationHovered) {
        fill(0, 0, 0);
    }
    else if (isSelected) {
        fill(0, 255, 0); // Change color to green
    }
    else if (d < 5) { // Adjust the radius (5) as needed
        fill(255, 0, 0); // Change color to red
    } else {
        fill(255, 255, 255); // Default color
    }

    ellipse(city.x, city.y, 10, 10);
}

function isDoubleRoute(route, claimedRoutes) {
    let firstIndex = claimedRoutes.indexOf(route);
    let secondIndex = -1;
    let isRouteDouble = false;
    let isSecondRoute = false;

    for (let i = 0; i < claimedRoutes.length; i++) {
        if (i !== firstIndex &&
            claimedRoutes[i].destination === route.destination &&
            claimedRoutes[i].origin === route.origin) {
            secondIndex = i;
            break;
        }
    }

    if (secondIndex != -1) {
        isRouteDouble = true;
    }

    if (secondIndex != -1 && secondIndex < firstIndex) {
        isSecondRoute = true;
    }
    return { isRouteDouble, isSecondRoute };
}

function drawClaimedRoute(route, isRouteDouble, isSecondRoute) {
    let point1 = 0;
    let point2 = 0;
    if (!isRouteDouble) {
        point1 = createVector(route.origin.x, route.origin.y);
        point2 = createVector(route.destination.x, route.destination.y);
    }
    else {
        //route is double
        let offSet = 4;
        if (Math.abs(route.origin.x - route.destination.x) < 20) {
            if (isSecondRoute) {
                point1 = createVector(route.origin.x + offSet, route.origin.y);
                point2 = createVector(route.destination.x + offSet, route.destination.y);
            }
            else {
                point1 = createVector(route.origin.x - offSet, route.origin.y);
                point2 = createVector(route.destination.x - offSet, route.destination.y);
            }
        }
        else{
            //the difference is between y
            if (isSecondRoute) {
                point1 = createVector(route.origin.x , route.origin.y + offSet);
                point2 = createVector(route.destination.x , route.destination.y + offSet);
            }
            else {
                point1 = createVector(route.origin.x, route.origin.y - offSet);
                point2 = createVector(route.destination.x, route.destination.y - offSet);
            }
        }

    }

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
        console.log(responseJson);

        if (responseJson.isValid) {
            await tryToClaimRoute(origin, destination);
        }
        else {
            showMessage(responseJson.message);
        }
    }
    else {
        console.error(response)
        showMessage(errorResponse);
    }
}

async function tryToClaimRoute(origin, destination) {
    let url = `http://localhost:5001/game/CanClaimRoute?playerIndex=${playerIndex}&origin=${origin}&&destination=${destination}`;

    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });

    if (response.ok) {
        let responseJson = await response.json();
        console.log(responseJson);
        let message = buildColorsWithWhichRouteCanBeClaimedMessage(responseJson.trainColors);
        showMessage(message, false, false);
    }
    else {
        let errorText = await response.text();
        showMessage(errorText)
        console.error(errorText)
    }
}

function changeStroke(playerColor) {
    strokeWeight(4);

    switch (playerColor) {
        case PlayerColor.Blue:
            stroke(0, 0, 255);
            return;
        case PlayerColor.Red:
            stroke(255, 0, 0);
            return;
        case PlayerColor.Green:
            stroke(0, 110, 0);
            return;
        case PlayerColor.Yellow:
            stroke(255, 255, 0);
            return;
        default:
            stroke(0, 0, 0);
    }

}

function resetStroke() {
    strokeWeight(1);
    stroke(0, 0, 0);
}
