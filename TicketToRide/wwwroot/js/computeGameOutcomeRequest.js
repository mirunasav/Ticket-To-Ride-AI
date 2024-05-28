export async function getGameOutcomeRequest() {
    let url = `http://localhost:5001/game/GetGameOutcome`;

    var response = await fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });

    if (response.ok) {
        let responseJson = await response.json();
        console.log(responseJson)
        return responseJson;
    }
    else {
        let errorResponse = await response.text();
        console.error(errorResponse);
        showMessage(errorResponse);
        return;
    }
}

