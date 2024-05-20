import { PlayerColor } from "./GameObjects.js";
import { getPlayerColorFromNumber } from "./getObjectsFromEnum.js";
import { emptyHtmlContainer } from "./utils.js";

export function initDisplayPlayerStatistics(players, playerIndex) {
    const currentPlayer = players[playerIndex];

    // Get the container to display player statistics
    const playerStatisticsContainer = emptyHtmlContainer('all-player-stats-container');
    // Loop through all players to create statistics
    for (const player of players) {
        let cssColor = getCSSColor(player.color);

        // Create a div for player statistics
        const playerStatsDiv = document.createElement('div');
        playerStatsDiv.classList.add('player-stats-container');

        // Player name and points
        const playerName = document.createElement('p');
        playerName.classList.add('player-name');

        playerName.textContent = `${player.name}: ${player.points} points`;

        if(player === currentPlayer){
            playerName.textContent += ' (You)';
        }

        playerStatsDiv.appendChild(playerName);

        // Train icon and remaining trains count
        const remainingTrainsContainer = document.createElement('div');
        remainingTrainsContainer.classList.add('player-stats-counts-container');
        playerStatsDiv.appendChild(remainingTrainsContainer);

        const trainIcon = document.createElement('i');
        trainIcon.classList.add('fa-solid', 'fa-train');
        trainIcon.style.color = cssColor;
        const trainsCount = document.createElement('span');
        trainsCount.textContent = player.remainingTrains;
        remainingTrainsContainer.appendChild(trainIcon);
        remainingTrainsContainer.appendChild(trainsCount);

        // Ticket icon and number of cards in hand
        const trainCardsContainer = document.createElement('div');
        trainCardsContainer.classList.add('player-stats-counts-container');
        playerStatsDiv.appendChild(trainCardsContainer);


        const ticketIcon = document.createElement('i');
        ticketIcon.classList.add('fa-solid', 'fa-ticket-alt');
        ticketIcon.style.color = cssColor;
        const cardsInHandCount = document.createElement('span');
        cardsInHandCount.textContent = player === currentPlayer
            ? player.hand.length
            : player.numberOfTrainCards;
        trainCardsContainer.appendChild(ticketIcon);
        trainCardsContainer.appendChild(cardsInHandCount);

        // Location icon and number of destination cards
        const destinationCardsContainer = document.createElement('div');
        destinationCardsContainer.classList.add('player-stats-counts-container');
        playerStatsDiv.appendChild(destinationCardsContainer);

        const locationIcon = document.createElement('i');
        locationIcon.classList.add('fa-solid', 'fa-map-marker-alt');
        locationIcon.style.color = cssColor;
        const destinationCardsCount = document.createElement('span');

        const completedDestinationCards = player === currentPlayer
            ? player.completedDestinationCards.length
            : '?';

        const pendingDestinationCards = player === currentPlayer
            ? player.pendingDestinationCards.length
            : player.numberOfPendingDestinationCards;

        const allDestinationCards = completedDestinationCards + pendingDestinationCards;

        destinationCardsCount.textContent = `${completedDestinationCards} / ${pendingDestinationCards}`;
        destinationCardsContainer.appendChild(locationIcon);
        destinationCardsContainer.appendChild(destinationCardsCount);

        // Add player statistics div to the container
        playerStatisticsContainer.appendChild(playerStatsDiv);
    }
}

function getCSSColor(color){
    switch(color){
        case PlayerColor.Red:
            return "red";
        case PlayerColor.Blue:
            return "blue";
        case PlayerColor.Yellow:
            return "yellow";
        case PlayerColor.Green:
            return "green";
        default:
            return "black";
    }
}
export function updatePlayerStatistics(playerIndex) {
    //only change points, train count, destination count, train cards count
}