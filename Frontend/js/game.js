window.addEventListener("DOMContentLoaded", async function () {
    let tableData;
    let gameData;
    let hasSelectedTile = false;
    let token = sessionStorage.getItem('authToken');
    let tableId = sessionStorage.getItem('tableID');

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
            console.error("Error fetching game data:", error);
        }
    }

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
        console.error("Error fetching table data:", error);
    }

    await getGameData();

    setInterval(getGameData, 1000);

    function loadFactoryTiles(tileFactory) {
        const currentPlayerId = sessionStorage.getItem("playerId");
        const displays = tileFactory.displays;
        const tableCenter = document.getElementById('tableCenter');

        displays.forEach((display, index) => {
            const factoryElement = document.getElementById(`tile${index + 1}`);
            if (factoryElement) {
                factoryElement.innerHTML = "";

                display.tiles.forEach(tile => {
                    const tileElement = document.createElement("div");
                    tileElement.className = "factory-tile";
                    tileElement.textContent = tile;
                    tileElement.style.backgroundColor = getColorForTile(tile);

                    factoryElement.appendChild(tileElement);

                    if (currentPlayerId === gameData.playerToPlayId && !hasSelectedTile) {
                        tileElement.addEventListener('click', async () => {
                            if (hasSelectedTile) return;
                            hasSelectedTile = true;

                            document.querySelectorAll('.factory-tile').forEach(el => {
                                const newEl = el.cloneNode(true);
                                el.parentNode.replaceChild(newEl, el);
                            });

                            Array.from(factoryElement.children).forEach(child => {
                                child.style.border = "none";
                            });

                            display.tiles.forEach((tileTypeInDisplay, tileIndex) => {
                                if (tileTypeInDisplay === tile) {
                                    const matchingTileDiv = factoryElement.children[tileIndex];
                                    matchingTileDiv.style.border = "3px solid green";
                                }
                            });

                            display.tiles.forEach((remainingTile) => {
                                if (remainingTile !== tile) {
                                    const tileElement = document.createElement("div");
                                    tileElement.className = "factory-tile";
                                    tileElement.textContent = remainingTile;
                                    tileElement.style.backgroundColor = getColorForTile(remainingTile);
                                    tableCenter.appendChild(tileElement);
                                }
                            });

                            try {
                                const response = await fetch(`https://localhost:5051/api/games/${tableData.gameId}/take-tiles`, {
                                    method: 'POST',
                                    headers: {
                                        'Content-Type': 'application/json',
                                        'Authorization': 'Bearer ' + token
                                    },
                                    body: JSON.stringify({
                                        "displayId": display.id,
                                        "tileType": tile
                                    })
                                });

                                if (!response.ok) {
                                    console.error('Fout bij versturen:', await response.text());
                                } else {
                                    console.log('Verzonden');

                                    // Verwijder alle tegels uit de huidige fabriekschijf (grafisch)
                                    factoryElement.innerHTML = "";

                                    console.log('Verzonden');
                                    await getGameData(); // <-- Direct gameData vernieuwen
                                    updatePlayerHighlights();

                                    // Activeer patroonlijnselectie met tegelkleur en aantal
                                    const aantalGeselecteerdeTegels = display.tiles.filter(t => t === tile).length;
                                    enablePatternLineSelection(gameData.id, getColorForTile(tile), aantalGeselecteerdeTegels);
                                }
                            } catch (err) {
                                console.error('Netwerkfout:', err);
                            }
                        });
                    }
                });
            }
        });
    }

    function getColorForTile(tile) {
        switch (tile) {
            case 11: return "#4160f9";
            case 12: return "#f5c305";
            case 13: return "#ff0000";
            case 14: return "#000000";
            case 15: return "#30D5C8";
            default: return "#adb5bd";
        }
    }

    if (gameData && gameData.tileFactory) {
        loadFactoryTiles(gameData.tileFactory);
        enablePatternLineSelection(gameData.id, null, token); // Voor nu null kleur
    }

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

    let aantalFabrieken = 5;
    if (aantalSpelers === 3) aantalFabrieken = 7;
    if (aantalSpelers === 4) aantalFabrieken = 9;

    for (let i = 1; i <= 9; i++) {
        const factory = document.getElementById(`tile${i}`);
        factory.style.display = i <= aantalFabrieken ? "flex" : "none";
    }

    tableData.seatedPlayers.forEach((player, index) => {
        const boardNumber = index + 1;
        const boardElement = document.getElementById(`board${boardNumber}`);

        if (boardElement) {
            let nameLabel = document.getElementById(`playerName${boardNumber}`);
            if (!nameLabel) {
                nameLabel = document.createElement("div");
                nameLabel.id = `playerName${boardNumber}`;
                nameLabel.style.textAlign = "center";
                nameLabel.style.fontWeight = "bold";
                nameLabel.style.marginBottom = "4px";
                boardElement.parentNode.insertBefore(nameLabel, boardElement);
            }
            nameLabel.textContent = player.name;
        }
    });

    let vorigePlayerToPlayId = null;

    function updatePlayerHighlights() {
        const currentPlayerId = sessionStorage.getItem("playerId");

        // Check of de beurt is veranderd
        if (gameData.playerToPlayId !== vorigePlayerToPlayId) {
            hasSelectedTile = false;
            vorigePlayerToPlayId = gameData.playerToPlayId;
        }

        tableData.seatedPlayers.forEach((player, index) => {
            const boardNumber = index + 1;
            const nameLabel = document.getElementById(`playerName${boardNumber}`);

            if (nameLabel) {
                nameLabel.style.color = "black";
                nameLabel.style.backgroundColor = "transparent";

                if (player.id === gameData.playerToPlayId) {
                    nameLabel.style.color = "white";
                    nameLabel.style.backgroundColor = "darkgreen";
                    nameLabel.style.padding = "4px";
                    nameLabel.style.borderRadius = "6px";
                }
            }
        });
    }


    function updatePlayerScore() {
        tableData.seatedPlayers.forEach((player, index) => {
            const boardNumber = index + 1;
            const boardElement = document.getElementById(`board${boardNumber}`);

            if (boardElement) {
                let nameLabel = document.getElementById(`playerScore${boardNumber}`);
                if (!nameLabel) {
                    nameLabel = document.createElement("div");
                    nameLabel.id = `playerScore${boardNumber}`;
                    nameLabel.style.textAlign = "center";
                    nameLabel.style.fontWeight = "bold";
                    nameLabel.style.marginBottom = "4px";
                    boardElement.parentNode.insertBefore(nameLabel, boardElement);
                }

                nameLabel.textContent = "score: " + player.board.score;
            }
        });
    }

    function enablePatternLineSelection(gameId, selectedTileColor, aantalTegels) {
        const patternLines = document.querySelectorAll(".pattern-line");

        patternLines.forEach((line) => {
            line.addEventListener("click", async () => {
                const patternLineIndex = parseInt(line.dataset.index);

                try {
                    const response = await fetch(`https://localhost:5051/api/Games/${gameId}/place-tiles-on-patternline`, {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        },
                        body: JSON.stringify({
                            patternLineIndex: patternLineIndex
                        })
                    });

                    if (response.ok) {
                        console.log(`Tegels geplaatst op patroonlijn ${patternLineIndex}`);

                        const tiles = line.querySelectorAll('.tile');
                        let filled = 0;
                        for (let tile of tiles) {
                            if (!tile.classList.contains('filled') && filled < aantalTegels) {
                                tile.classList.add('filled');
                                tile.style.backgroundColor = selectedTileColor;
                                filled++;
                            }
                        }
                    } else {
                        const errorText = await response.text();
                        console.error("Server error:", errorText);
                        alert("Fout bij plaatsen van tegel: " + errorText);
                    }
                } catch (err) {
                    console.error("Netwerkfout:", err);
                    alert("Kon geen verbinding maken met server.");
                }
            });
        });
    }
});
