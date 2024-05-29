import { emptyHtmlContainer } from "./utils.js";

export function displayGameLog(gameLog){
    console.log('gamelog', gameLog)
    let gameLogContainer = emptyHtmlContainer('game-log-container__lines');
    for(const line of gameLog.gameLogLines){
        let lineDiv = getGameLogLineElement(line);
        console.log(lineDiv)
        gameLogContainer.appendChild(lineDiv);
    }
}

function getGameLogLineElement(line){
    const lineDiv = document.createElement('div');
    lineDiv.classList.add('game-log-line');

    const textContent = `[#${line.index}] ${line.message}`;
    lineDiv.textContent = textContent;
    return lineDiv;
}

