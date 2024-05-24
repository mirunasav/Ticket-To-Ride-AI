export async function getExistingGameRequest(playerIndex) {
    let url = `http://localhost:5001/game?playerIndex=${playerIndex}`;

    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });

    return response;
}

export async function getNewGameRequest(numberOfPlayers, playerTypes) {
    let url = `http://localhost:5001/game/newGame?numberOfPlayers=${numberOfPlayers}`;

    let requestBody = {
        numberOfPlayers : numberOfPlayers,
        playerTypes : playerTypes
    };
    
    console.log(requestBody);

    var response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
    });
    
    return response;
}

