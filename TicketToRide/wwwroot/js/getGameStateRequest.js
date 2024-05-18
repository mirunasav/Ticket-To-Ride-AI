export async function getGameStateRequest() {
    let url = `http://localhost:5001/game/GetGameState`;

    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });

    if (response.ok) {
        let responseJson = await response.json();
        return responseJson;
    }
    else {
        let errorResponse = await response.text();
        console.error(errorResponse);
        return;
    }
}

