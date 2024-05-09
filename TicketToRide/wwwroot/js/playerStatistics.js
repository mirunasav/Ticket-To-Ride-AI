export function initDisplayPlayerStatistics(players, playerIndex){
    const currentPlayer = players[playerIndex];

    // Get the container to display player statistics
    const playerStatisticsContainer = document.getElementById('all-player-stats-container');

    // Loop through all players to create statistics
    for (const player of players) {
        // Create a div for player statistics
        const playerStatsDiv = document.createElement('div');
        playerStatsDiv.classList.add('player-stats-container');

        // Player name and points
        const playerName = document.createElement('p');
        playerName.classList.add('player-name');

        playerName.textContent = `${player.name}: ${player.points} points`;
        playerStatsDiv.appendChild(playerName);

        // Train icon and remaining trains count
        const remainingTrainsContainer = document.createElement('div');
        remainingTrainsContainer.classList.add('player-stats-counts-container');
        playerStatsDiv.appendChild(remainingTrainsContainer);

        const trainIcon = document.createElement('i');
        trainIcon.classList.add('fa-solid', 'fa-train');
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
        const cardsInHandCount = document.createElement('span');
        cardsInHandCount.textContent = player.hand.length;
        trainCardsContainer.appendChild(ticketIcon);
        trainCardsContainer.appendChild(cardsInHandCount);

        // Location icon and number of destination cards
        const destinationCardsContainer = document.createElement('div');
        destinationCardsContainer.classList.add('player-stats-counts-container');
        playerStatsDiv.appendChild(destinationCardsContainer);

        const locationIcon = document.createElement('i');
        locationIcon.classList.add('fa-solid', 'fa-map-marker-alt');
        const destinationCardsCount = document.createElement('span');

        const completedDestinationCards = player.completedDestinationCards.length;
        const pendingDestinationCards = player.pendingDestinationCards.length;
        const allDestinationCards = completedDestinationCards + pendingDestinationCards;

        destinationCardsCount.textContent = `${completedDestinationCards} / ${pendingDestinationCards}`;
        destinationCardsContainer.appendChild(locationIcon);
        destinationCardsContainer.appendChild(destinationCardsCount);

        // Add player statistics div to the container
        playerStatisticsContainer.appendChild(playerStatsDiv);
    }
}

export function updatePlayerStatistics(playerIndex){
    //only change points, train count, destination count, train cards count
}