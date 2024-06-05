export async function openCreateNewGameSweetAlert() {
    return new Promise((resolve, reject) => {
        let numberOfPlayers = 0;
        const playerTypes = [];

        Swal.fire({
            title: 'Create a Game',
            html: `
                <form id="gameForm">
                    <label for="numberOfPlayers">Number of Players:</label>
                    <input type="number" id="numberOfPlayersInput" name="numberOfPlayers" min="2" max="4" class="swal2-input">
                    <div id="playerFields"></div>
                    <button type="submit" id="submitButton" disabled class="swal2-confirm swal2-styled">Create</button>
                </form>
            `,
            showConfirmButton: false,
            didOpen: () => {
                const numberOfPlayersInput = document.getElementById('numberOfPlayersInput');
                const playerFieldsContainer = document.getElementById('playerFields');
                const submitButton = document.getElementById('submitButton');

                const createPlayerFields = (numPlayers) => {
                    playerFieldsContainer.innerHTML = '';
                    for (let i = 1; i <= numPlayers; i++) {
                        const playerField = document.createElement('div');
                        playerField.innerHTML = `
                            <label for="player${i}Type">Player ${i} Type:</label>
                            <select id="player${i}Type" name="player${i}Type" class="swal2-input">
                                <option value="">Select type</option>
                                <option value="Human">Human</option>
                                <option value="RandomDecisionBot">Random Decision Bot</option>
                                <option value="PseudoRandomBot">Pseudo Random Bot</option>
                                <option value="SimpleStrategyBot">Simple Strategy Bot</option>
                                <option value="LongestRouteBot">Longest Route Bot</option>
                                <option value="CardHoarderBot">Card Hoarder Bot</option>
                            </select>
                        `;
                        playerFieldsContainer.appendChild(playerField);
                    }
                };

                const validateForm = () => {
                    const numPlayers = numberOfPlayersInput.value;
                    if (numPlayers < 2 || numPlayers > 4) {
                        submitButton.disabled = true;
                        return;
                    }
                    const playerSelects = playerFieldsContainer.querySelectorAll('select');
                    const allSelected = Array.from(playerSelects).every(select => select.value !== '');
                    submitButton.disabled = !allSelected;
                };

                numberOfPlayersInput.addEventListener('input', (e) => {
                    const numPlayers = e.target.value;
                    if (numPlayers >= 2 && numPlayers <= 4) {
                        createPlayerFields(numPlayers);
                        validateForm();
                    } else {
                        playerFieldsContainer.innerHTML = '';
                        submitButton.disabled = true;
                    }
                });

                playerFieldsContainer.addEventListener('change', validateForm);

                document.getElementById('gameForm').addEventListener('submit', function (e) {
                    e.preventDefault();
                    const formData = new FormData(e.target);

                    formData.forEach((value, key) => {
                        if (key === 'numberOfPlayers') {
                            numberOfPlayers = value;
                        }
                        if (key.startsWith('player')) {
                            playerTypes.push(value);
                        }
                    });

                    resolve({ numberOfPlayers, playerTypes });
                    Swal.close();
                });
            },
            willClose: () => {
                reject();
            }
        });
    });
}