export function emptyHtmlContainer(containerId){
    let container = document.getElementById(`${containerId}`);
    container.innerHTML = '';
    return container;
}

export function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
