import { getCityFromNumber } from "./getObjectsFromEnum.js";

export function displayPlayerDestinationCards(player){
    let pendingDestinationCards = player.pendingDestinationCards;
    let completedDestinationCards = player.completedDestinationCards;
    
    const pendingContainer = document.getElementById('pending-destinations-container');
    const completedContainer = document.getElementById('completed-destinations-container');

    for(const card of pendingDestinationCards){
        const cardDiv = createDestinationCardElement(card);
        pendingContainer.appendChild(cardDiv);
    }

    
    for(const card of completedDestinationCards){
        const cardDiv = createDestinationCardElement(card);
        completedContainer.appendChild(cardDiv);
    }
}

function createDestinationCardElement(card) {
    const cardDiv = document.createElement('div');
    cardDiv.classList.add('destination-card');

    const originCity = getCityFromNumber(card.origin);
    const destinationCity = getCityFromNumber(card.destination);

    const originDestination = document.createElement('strong');

    originDestination.textContent = `${originCity.name} - ${destinationCity.name}`;
    cardDiv.appendChild(originDestination);

    const points = document.createElement('p');
    points.textContent = `Points: ${card.pointValue}`;
    cardDiv.appendChild(points);

    return cardDiv;
}