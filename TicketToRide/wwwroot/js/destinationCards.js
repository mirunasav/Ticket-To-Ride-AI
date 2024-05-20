import { getCityFromNumber } from "./getObjectsFromEnum.js";
import { emptyHtmlContainer } from "./utils.js";

export let hoveredCityNames = [];

export function displayPlayerDestinationCards(player) {
    let pendingDestinationCards = player.pendingDestinationCards;
    let completedDestinationCards = player.completedDestinationCards;

    let pendingContainer = emptyHtmlContainer('pending-destinations-container');
    let completedContainer = emptyHtmlContainer('completed-destinations-container');

    for (const card of pendingDestinationCards) {
        const cardDiv = createDestinationCardElement(card);
        pendingContainer.appendChild(cardDiv);
    }

    for (const card of completedDestinationCards) {
        const cardDiv = createDestinationCardElement(card);
        completedContainer.appendChild(cardDiv);
    }

    addEventListenerToPendingAndCompletedTitles(pendingDestinationCards, completedDestinationCards);
}

function addEventListenerToPendingAndCompletedTitles(pendingDestinationCards, completedDestinationCards){
    let pendingDestinationsTitle = document.getElementById('pending-destination-cards-title');
    let completedDestinationsTitle = document.getElementById('completed-destination-cards-title');

    pendingDestinationsTitle.addEventListener('mouseover', () => {
        hoveredCityNames = [];
        for (const card of pendingDestinationCards) {
            const originCity = getCityFromNumber(card.origin);
            const destinationCity = getCityFromNumber(card.destination);
            hoveredCityNames.push(originCity.name);
            hoveredCityNames.push(destinationCity.name);
        }
        pendingDestinationsTitle.classList.add('hovered-destination-card');
    });
    
    pendingDestinationsTitle.addEventListener('mouseout', () => {
        hoveredCityNames = [];
        pendingDestinationsTitle.classList.remove('hovered-destination-card');
    });

    
    completedDestinationsTitle.addEventListener('mouseover', () => {
        hoveredCityNames = [];
        for (const card of completedDestinationCards) {
            const originCity = getCityFromNumber(card.origin);
            const destinationCity = getCityFromNumber(card.destination);
            hoveredCityNames.push(originCity.name);
            hoveredCityNames.push(destinationCity.name);
        }
        completedDestinationsTitle.classList.add('hovered-destination-card');
    });
    
    completedDestinationsTitle.addEventListener('mouseout', () => {
        hoveredCityNames = [];
        completedDestinationsTitle.classList.remove('hovered-destination-card');
    });
}
function createDestinationCardElement(card) {
    const cardDiv = document.createElement('div');
    cardDiv.classList.add('destination-card');

    const originCity = getCityFromNumber(card.origin);
    const destinationCity = getCityFromNumber(card.destination);

    const originDestination = document.createElement('strong');

    originDestination.textContent = `${originCity.name} - ${destinationCity.name}`;
    cardDiv.appendChild(originDestination);

    originDestination.addEventListener('mouseover', () => {
        hoveredCityNames = [originCity.name, destinationCity.name];
        originDestination.classList.add('hovered-destination-card');
    });

    originDestination.addEventListener('mouseout', () => {
        originDestination.classList.remove('hovered-destination-card');
        hoveredCityNames = [];
    });

    const points = document.createElement('p');
    points.textContent = `Points: ${card.pointValue}`;
    cardDiv.appendChild(points);

    return cardDiv;
}

export async function emptyHoveredCityNames(){
    hoveredCityNames = [];
}