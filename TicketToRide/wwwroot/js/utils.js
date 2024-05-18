export function emptyHtmlContainer(containerId){
    let container = document.getElementById(`${containerId}`);
    container.innerHTML = '';
    return container;
}