window.addEventListener("DOMContentLoaded", async function () {
    let tableData;
    let gameData;
    let token = sessionStorage.getItem('authToken');
    let tableId = sessionStorage.getItem('tableID');
    //dit moet worden uitgevoerd bij het inladen van de pagina
    try {
        const response = await fetch(`https://localhost:5051/api/Tables/${tableId}`, {
            method: "GET",
            headers: {
                'Authorization': "Bearer " + token
            }
        });
        if (response.ok) {
            tableData = await response.json();
        }
    } catch (error) {
    }
    try {
        const response = await fetch(`https://localhost:5051/api/games/${tableData.gameId}`, {
            method: "GET",
            headers: {
                'Authorization': "Bearer " + token
            }
        });
        if (response.ok) {
            gameData = await response.json();
        }
    } catch (error) {
    }
    console.log(token);
    console.log(tableData.gameId);
    setInterval(getGameData, 100);

// tableInfo
    async function getGameData() {
        try {
            const response = await fetch(`https://localhost:5051/api/games/${tableData.gameId}`, {
                method: "GET",
                headers: {
                    'Authorization': "Bearer " + token
                }
            });
            if (response.ok) {
                gameData = await response.json();

                updatePlayerHighlights();
                updatePlayerScore();
            }
        } catch (error) {
        }
    }
    function loadFactoryTiles(tileFactory) {
        const displays = tileFactory.displays;
        displays.forEach((display, index) => {
            const factoryElement = document.getElementById(`tile${index + 1}`);
            if (factoryElement) {
                factoryElement.innerHTML = ""; // Leeg de fabriek
                display.tiles.forEach(tile => {
                    const tileElement = document.createElement("div");
                    tileElement.className = "factory-tile";
                    tileElement.textContent = tile; // Voeg tegeltype toe als tekst (optioneel)
                    tileElement.style.backgroundColor = getColorForTile(tile); // Stel kleur in
                    factoryElement.appendChild(tileElement);
                });
            }
        });
    }

    // Functie om tegeltype om te zetten in een kleur (voorbeeldkleuren)
    function getColorForTile(tile) {
        switch (tile) {
            case 11: return "#f94144"; // Rood
            case 13: return "#f3722c"; // Oranje
            case 14: return "#f9c74f"; // Geel
            case 15: return "#43aa8b"; // Groen
            case 0: return "#495057";  // Leeg (grijs)
            default: return "#adb5bd"; // Standaardkleur
        }
    }

    if (gameData && gameData.tileFactory) {
        loadFactoryTiles(gameData.tileFactory);
    }

    // Interval om game data regelmatig te verversen
    setInterval(async function () {
        try {
            const response = await fetch(`https://localhost:5051/api/games/${tableData.gameId}`, {
                method: "GET",
                headers: {
                    'Authorization': "Bearer " + token
                }
            });
            if (response.ok) {
                gameData = await response.json();
                loadFactoryTiles(gameData.tileFactory); // Ververs fabrieken
            }
        } catch (error) {
            console.error("Error updating game data:", error);
        }
    }, 1000); // Elke seconde
    // Toon borden
    const aantalSpelers = parseInt(sessionStorage.getItem("aantalSpelers"));
    if (!aantalSpelers || aantalSpelers < 2 || aantalSpelers > 4) {
        console.error("Ongeldig aantal spelers:", aantalSpelers);
        return;
    }

    for (let i = 1; i <= 4; i++) {
        const bord = document.getElementById(`board${i}`);
        if (bord) {
            bord.style.display = i <= aantalSpelers ? "grid" : "none";
        }
    }

    // Fabrieken tonen
    let aantalFabrieken = 5;
    if (aantalSpelers === 3) aantalFabrieken = 7;
    if (aantalSpelers === 4) aantalFabrieken = 9;

    for (let i = 1; i <= 9; i++) {
        const factory = document.getElementById(`tile${i}`);
        factory.style.display = i <= aantalFabrieken ? "flex" : "none";
    }

    // Voeg speler-namen toe boven hun borden
    tableData.seatedPlayers.forEach((player, index) => {
        const boardNumber = index + 1;
        const boardElement = document.getElementById(`board${boardNumber}`);

        if (boardElement) {
            // Maak een label aan als deze nog niet bestaat
            let nameLabel = document.getElementById(`playerName${boardNumber}`);
            if (!nameLabel) {
                nameLabel = document.createElement("div");
                nameLabel.id = `playerName${boardNumber}`;
                nameLabel.style.textAlign = "center";
                nameLabel.style.fontWeight = "bold";
                nameLabel.style.marginBottom = "4px";

                boardElement.parentNode.insertBefore(nameLabel, boardElement);
            }

            // Zet de naam van de speler
            nameLabel.textContent = player.name;
        }
    });

    //markeer de huidige speler
    function updatePlayerHighlights() {
        tableData.seatedPlayers.forEach((player, index) => {
            const boardNumber = index + 1;
            const nameLabel = document.getElementById(`playerName${boardNumber}`);

            if (nameLabel) {
                // Reset stijl
                nameLabel.style.color = "black";
                nameLabel.style.backgroundColor = "transparent";

                // Markeer huidige speler
                if (player.id === gameData.playerToPlayId) {
                    nameLabel.style.color = "white";
                    nameLabel.style.backgroundColor = "darkgreen";
                    nameLabel.style.padding = "4px";
                    nameLabel.style.borderRadius = "6px";
                }
            }
        });
    }

    //voeg de huidige score toe
    function updatePlayerScore() {
    tableData.seatedPlayers.forEach((player, index) => {
        const boardNumber = index + 1;
        const boardElement = document.getElementById(`board${boardNumber}`);

        if (boardElement) {
            // Maak een label aan als deze nog niet bestaat
            let nameLabel = document.getElementById(`playerScore${boardNumber}`);
            if (!nameLabel) {
                nameLabel = document.createElement("div");
                nameLabel.id = `playerScore${boardNumber}`;
                nameLabel.style.textAlign = "center";
                nameLabel.style.fontWeight = "bold";
                nameLabel.style.marginBottom = "4px";

                boardElement.parentNode.insertBefore(nameLabel, boardElement);
            }

            // Zet de naam van de speler
            nameLabel.textContent ="score: " + player.board.score;
        }
    });
}
});
