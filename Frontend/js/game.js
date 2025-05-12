window.addEventListener("DOMContentLoaded", async function () {
    let tableData;
    let gameData;
    let hasSelectedTile = false;
    let token = sessionStorage.getItem('authToken');
    let tableId = sessionStorage.getItem('tableID');
    let vorigePlayerToPlayId = null;
    // Permanente opslag voor lokale pattern line wijzigingen
    // Structure: { playerId: { patternLineIndex: { color: string, count: number } } }
    let permanentLocalChanges = { playerPatternLines: {} };
    
    // Lokale informatie over huidige tegelplaatsing
    let currentPlacement = {
        patternLineIndex: -1,
        tileColor: null,
        tileCount: 0,
        active: false
    };

    // Object om bij te houden welke tegels recent zijn geplaatst
    let recentPlacedTiles = {
        playerId: null,
        patternLineIndex: -1,
        tileColor: null,
        tileCount: 0,
        timestamp: 0
    };

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
                console.log("Game data ontvangen:", gameData);
                
                // Log alle tegellocaties
                logTileLocations();
                
                console.log("Player to play:", gameData.playerToPlayId);
                updatePlayerHighlights();
                updatePlayerScore();
                
                // Update all boards
                const boards = document.querySelectorAll('.azul-board');
                gameData.players.forEach((player, index) => {
                    if (boards[index]) {
                        updateBoard(player.board, boards[index]);
                    }
                });

                if (gameData && gameData.tileFactory) {
                    loadFactoryTiles(gameData.tileFactory);
                }
                updateTableCenter();
            }
        } catch (error) {
            console.error("Error fetching game data:", error);
        }
    }

    function logTileLocations() {
        console.log("=== TILE LOCATIONS ===");
        
        // Log factory displays
        console.log("--- Factory Displays ---");
        if (gameData.tileFactory && gameData.tileFactory.displays) {
            gameData.tileFactory.displays.forEach((display, index) => {
                console.log(`Factory ${index + 1}:`, display.tiles);
            });
        }
        
        // Log table center
        console.log("--- Table Center ---");
        if (gameData.tileFactory && gameData.tileFactory.tableCenter) {
            console.log("Center tiles:", gameData.tileFactory.tableCenter.tiles);
        }
        
        // Log player boards
        console.log("--- Player Boards ---");
        gameData.players.forEach((player, playerIndex) => {
            console.log(`\nPlayer ${playerIndex + 1} (${player.name}):`);
            
            // Pattern lines
            console.log("Pattern Lines:");
            player.board.patternLines.forEach((line, lineIndex) => {
                console.log(`Line ${lineIndex + 1}:`, {
                    tiles: line.tiles,
                    numberOfTiles: line.numberOfTiles,
                    tileType: line.tileType
                });
            });
            
            // Wall
            console.log("Wall:");
            player.board.wall.forEach((row, rowIndex) => {
                console.log(`Row ${rowIndex + 1}:`, row.map(spot => ({
                    type: spot.type,
                    hasTile: spot.hasTile
                })));
            });
            
            // Floor line
            console.log("Floor Line:", player.board.floorLine.map(spot => ({
                type: spot.type,
                hasTile: spot.hasTile
            })));
        });
        
        console.log("==================");
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
    // Start automatische vernieuwing en sla het interval op
    autoRefreshInterval = setInterval(getGameData, 1000);

    function loadFactoryTiles(tileFactory) {
        console.log("Factory tiles worden opnieuw geladen", tileFactory);
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
                            
                            // Pauzeer automatische vernieuwing wanneer een tegel is geselecteerd
                            clearInterval(autoRefreshInterval);

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
                                    // Reset if error
                                    hasSelectedTile = false;
                                    Array.from(factoryElement.children).forEach(child => {
                                        child.style.border = "none";
                                    });
                                } else {
                                    // Don't clear the factory yet, wait until placement
                                    // Keep the selected tiles visible with border
                                    // await getGameData(); // Comment this out to prevent refreshing
                                    if (gameData) {
                                        updatePlayerHighlights();
                                    }
                                    const aantalGeselecteerdeTegels = display.tiles.filter(t => t === tile).length;
                                    enablePatternLineSelection(gameData.id, getColorForTile(tile), aantalGeselecteerdeTegels);
                                }
                            } catch (err) {
                                console.error('Netwerkfout:', err);
                                // Reset if error
                                hasSelectedTile = false;
                                Array.from(factoryElement.children).forEach(child => {
                                    child.style.border = "none";
                                });
                            }
                        });
                    }
                });
            }
        });
    }

    function getColorForTile(tile) {
        switch (tile) {
            case 0: return "#adb5bd"; // StartingTile
            case 11: return "#4169E1"; // PlainBlue
            case 12: return "#FFD700"; // YellowRed
            case 13: return "#FF0000"; // PlainRed
            case 14: return "#000000"; // BlackBlue
            case 15: return "#40E0D0"; // WhiteTurquoise
            default: return "#adb5bd"; // Default gray for unknown types
        }
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


    function updatePlayerHighlights() {
        const currentPlayerId = sessionStorage.getItem("playerId");

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
            const newLine = line.cloneNode(true);
            line.parentNode.replaceChild(newLine, line);
        });
        
        const freshPatternLines = document.querySelectorAll(".pattern-line");
        freshPatternLines.forEach((line) => {
            line.addEventListener("click", async () => {
                if (!hasSelectedTile) return;
                
                const patternLineIndex = parseInt(line.dataset.index);
                
                // Get current player's board
                const currentPlayer = gameData.players.find(p => p.id === sessionStorage.getItem("playerId"));
                if (!currentPlayer || !currentPlayer.board) {
                    console.error("Could not find current player's board");
                    return;
                }

                // Convert color to tile type
                let selectedTileType = null;
                for (let i = 11; i <= 15; i++) {
                    if (getColorForTile(i) === selectedTileColor) {
                        selectedTileType = i;
                        break;
                    }
                }

                // Check if the wall already has this color in the corresponding row
                const wallRow = currentPlayer.board.wall[patternLineIndex];
                const hasColorInWall = wallRow.some(spot => spot.type === selectedTileType && spot.hasTile);
                
                if (hasColorInWall) {
                    // If color exists in wall, automatically place on floor line
                    try {
                        const floorLineResponse = await fetch(`https://localhost:5051/api/Games/${gameId}/place-tiles-on-floorline`, {
                            method: "POST",
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': 'Bearer ' + token
                            }
                        });

                        if (!floorLineResponse.ok) {
                            console.error("Failed to place tiles on floor line:", await floorLineResponse.text());
                        }

                        hasSelectedTile = false;
                        await getGameData();
                        updatePlayerHighlights();
                        autoRefreshInterval = setInterval(getGameData, 1000);
                        return;
                    } catch (err) {
                        console.error("Network error:", err);
                        hasSelectedTile = false;
                        autoRefreshInterval = setInterval(getGameData, 1000);
                        return;
                    }
                }

                try {
                    // Try to place on pattern line
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

                    if (!response.ok) {
                        // If pattern line placement fails, place on floor line
                        const floorLineResponse = await fetch(`https://localhost:5051/api/Games/${gameId}/place-tiles-on-floorline`, {
                            method: "POST",
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': 'Bearer ' + token
                            }
                        });

                        if (!floorLineResponse.ok) {
                            console.error("Failed to place tiles on floor line:", await floorLineResponse.text());
                        }
                    }

                    hasSelectedTile = false;
                    await getGameData();
                    updatePlayerHighlights();
                    autoRefreshInterval = setInterval(getGameData, 1000);

                } catch (err) {
                    console.error("Network error:", err);
                    hasSelectedTile = false;
                    autoRefreshInterval = setInterval(getGameData, 1000);
                }
            });
        });
    }

    function updatePatternLines(gameData) {
        gameData.players.forEach((player, index) => {
            const board = document.querySelectorAll('.azul-board')[index];
            const patternLines = board.querySelectorAll('.pattern-line');

            // Update pattern lines
            player.board.patternLines.forEach((line, lineIndex) => {
                const tiles = patternLines[lineIndex].querySelectorAll('.tile');
                const totalSpaces = tiles.length;

                // Clear all tiles first
                tiles.forEach(tile => {
                    tile.className = 'tile';
                    tile.textContent = '';
                });

                // Fill tiles from right to left if there are tiles
                if (line.numberOfTiles > 0 && line.tileType) {
                    for (let i = totalSpaces - 1; i >= totalSpaces - line.numberOfTiles; i--) {
                        tiles[i].classList.add(`tile${line.tileType}`);
                        tiles[i].textContent = line.tileType;
                    }
                }
            });
        });
    }

    function updateTableCenter() {
        if (!gameData || !gameData.tileFactory) {
            console.log("Geen gameData of tileFactory beschikbaar");
            return;
        }

        const tableCenter = document.getElementById('tableCenter');
        tableCenter.innerHTML = "";

        let centerTiles = [];
        let hasStartTile = false;

        if (gameData.tileFactory.centerTiles && Array.isArray(gameData.tileFactory.centerTiles)) {
            centerTiles = gameData.tileFactory.centerTiles;
        } else if (gameData.tileFactory.tableCenter?.tiles) {
            centerTiles = gameData.tileFactory.tableCenter.tiles;
        } else if (gameData.tileFactory.center?.tiles) {
            centerTiles = gameData.tileFactory.center.tiles;
        } else if (gameData.tableCenter?.tiles) {
            centerTiles = gameData.tableCenter.tiles;
        }

        // Check for starting tile
        hasStartTile = centerTiles.includes(10);

        // Create info container for selected tiles and messages
        const infoContainer = document.createElement("div");
        infoContainer.id = "tableCenterInfo";
        infoContainer.style.textAlign = "center";
        infoContainer.style.marginBottom = "10px";
        tableCenter.appendChild(infoContainer);

        // Create selected tiles info container
        const selectedCountContainer = document.createElement("div");
        selectedCountContainer.id = "selectedTilesInfo";
        selectedCountContainer.style.textAlign = "center";
        selectedCountContainer.style.fontWeight = "bold";
        selectedCountContainer.style.marginTop = "8px";
        selectedCountContainer.style.marginBottom = "8px";
        selectedCountContainer.style.display = "none";
        infoContainer.appendChild(selectedCountContainer);

        // Create warning container for floor line
        const warningContainer = document.createElement("div");
        warningContainer.id = "floorLineWarning";
        warningContainer.style.color = "#d9534f";
        warningContainer.style.display = "none";
        warningContainer.style.marginTop = "5px";
        infoContainer.appendChild(warningContainer);

        // Group tiles by type (excluding starting tile)
        const tilesByType = {};
        centerTiles.forEach(tile => {
            if (tile !== 10) {
                if (!tilesByType[tile]) {
                    tilesByType[tile] = 1;
                } else {
                    tilesByType[tile]++;
                }
            }
        });

        // Add starting tile if present
        if (hasStartTile) {
            const startTileContainer = document.createElement("div");
            startTileContainer.className = "start-tile-container";
            startTileContainer.style.marginBottom = "10px";
            startTileContainer.style.padding = "5px";
            startTileContainer.style.backgroundColor = "#f8f9fa";
            startTileContainer.style.border = "1px solid #dee2e6";
            startTileContainer.style.borderRadius = "4px";

            const startTileElement = document.createElement("div");
            startTileElement.className = "factory-tile";
            startTileElement.textContent = "Start";
            startTileElement.style.backgroundColor = "#adb5bd";
            startTileElement.style.border = "2px dashed black";
            startTileElement.style.fontWeight = "bold";
            startTileElement.title = "Starttegel: De eerste speler die tegels uit het midden pakt, ontvangt deze tegel op de vloerlijn.";

            const startTileText = document.createElement("div");
            startTileText.style.marginTop = "5px";
            startTileText.style.fontSize = "0.9em";
            startTileText.style.color = "#666";
            startTileText.innerHTML = "⚠️ De starttegel gaat automatisch naar je vloerlijn als je tegels uit het midden pakt";

            startTileContainer.appendChild(startTileElement);
            startTileContainer.appendChild(startTileText);
            tableCenter.appendChild(startTileContainer);
        }

        // Add grouped tiles
        Object.keys(tilesByType).forEach(tileType => {
            const count = tilesByType[tileType];
            const color = getColorForTile(parseInt(tileType));
            
            const tileGroupContainer = document.createElement("div");
            tileGroupContainer.className = "tile-group";
            tileGroupContainer.style.display = "inline-block";
            tileGroupContainer.style.margin = "5px";
            tileGroupContainer.style.position = "relative";
            
            const countBadge = document.createElement("div");
            countBadge.className = "tile-count";
            countBadge.textContent = count;
            countBadge.style.position = "absolute";
            countBadge.style.top = "-8px";
            countBadge.style.right = "-8px";
            countBadge.style.backgroundColor = "white";
            countBadge.style.border = "1px solid black";
            countBadge.style.borderRadius = "50%";
            countBadge.style.width = "20px";
            countBadge.style.height = "20px";
            countBadge.style.textAlign = "center";
            countBadge.style.lineHeight = "18px";
            countBadge.style.fontWeight = "bold";
            
            const tileElement = document.createElement("div");
            tileElement.className = "factory-tile";
            tileElement.textContent = tileType;
            tileElement.style.backgroundColor = color;
            
            const currentPlayerId = sessionStorage.getItem("playerId");
            if (currentPlayerId === gameData.playerToPlayId && !hasSelectedTile) {
                tileElement.style.cursor = "pointer";
                tileElement.addEventListener('click', async () => {
                    if (hasSelectedTile) return;
                    hasSelectedTile = true;
                    
                    clearInterval(autoRefreshInterval);
                    tileElement.style.border = "3px solid green";
                    
                    selectedCountContainer.style.display = "block";
                    selectedCountContainer.textContent = `Geselecteerd: ${count} ${color} tegel(s)`;
                    selectedCountContainer.style.color = color;

                    // Show starting tile warning if applicable
                    if (hasStartTile) {
                        warningContainer.style.display = "block";
                        warningContainer.innerHTML = `
                            <div style="margin-top: 10px; padding: 8px; background-color: #fff3cd; border: 1px solid #ffeeba; border-radius: 4px;">
                                <strong>⚠️ Let op:</strong> Je bent de eerste die tegels uit het midden pakt.<br>
                                De starttegel wordt automatisch naar je vloerlijn verplaatst.
                            </div>
                        `;
                    }
                    
                    try {
                        const response = await fetch(`https://localhost:5051/api/games/${gameData.id}/take-tiles`, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': 'Bearer ' + token
                            },
                            body: JSON.stringify({
                                "displayId": gameData.tileFactory.tableCenter.id,
                                "tileType": parseInt(tileType)
                            })
                        });

                        if (!response.ok) {
                            console.error('Fout bij versturen:', await response.text());
                            hasSelectedTile = false;
                            tileElement.style.border = "none";
                            selectedCountContainer.style.display = "none";
                            warningContainer.style.display = "none";
                            autoRefreshInterval = setInterval(getGameData, 1000);
                        } else {
                            const aantalGeselecteerdeTegels = count;
                            enablePatternLineSelection(gameData.id, color, aantalGeselecteerdeTegels);
                        }
                    } catch (err) {
                        console.error('Netwerkfout:', err);
                        hasSelectedTile = false;
                        tileElement.style.border = "none";
                        selectedCountContainer.style.display = "none";
                        warningContainer.style.display = "none";
                        autoRefreshInterval = setInterval(getGameData, 1000);
                    }
                });
            }
            
            tileGroupContainer.appendChild(tileElement);
            tileGroupContainer.appendChild(countBadge);
            tableCenter.appendChild(tileGroupContainer);
        });
    }

    function updateFloorLines(gameData) {
        gameData.players.forEach((player, index) => {
            const board = document.querySelectorAll('.azul-board')[index];
            const floorLine = board.querySelector('.floor-line');
            
            if (floorLine && player.board.floorLine) {
                const floorTiles = floorLine.querySelectorAll('.tile');
                
                // Clear floor line
                floorTiles.forEach(tile => {
                    tile.className = 'tile';
                    tile.textContent = '';
                });
                
                // Fill floor line with tiles from the data
                player.board.floorLine.forEach((spot, i) => {
                    if (spot.hasTile) {
                        if (spot.type === 0) { // Starting tile
                            floorTiles[i].style.backgroundColor = '#adb5bd';
                            floorTiles[i].textContent = 'Start';
                            floorTiles[i].style.fontWeight = 'bold';
                            floorTiles[i].style.border = '2px dashed black';
                        } else if (spot.type !== null) {
                            floorTiles[i].classList.add(`tile${spot.type}`);
                            floorTiles[i].textContent = spot.type;
                        }
                    }
                });
            }
        });
    }

    function updateBoard(board, boardElement) {
        if (!board) {
            console.error("No board data available");
            return;
        }

        // Update pattern lines
        const patternLines = boardElement.querySelectorAll('.pattern-line');
        patternLines.forEach((line, lineIndex) => {
            const tiles = line.querySelectorAll('.tile');
            const patternLine = board.patternLines[lineIndex];
            
            // Clear all tiles first
            tiles.forEach(tile => {
                tile.classList.remove('filled');
                tile.style.backgroundColor = '';
                tile.dataset.tileType = '';
                tile.textContent = '';
            });

            // Fill tiles based on pattern line data
            if (patternLine && patternLine.tiles) {
                patternLine.tiles.forEach((tile, tileIndex) => {
                    if (tile && tileIndex < tiles.length) {
                        tiles[tileIndex].classList.add('filled');
                        tiles[tileIndex].style.backgroundColor = getColorForTile(tile.type);
                        tiles[tileIndex].dataset.tileType = tile.type;
                        tiles[tileIndex].textContent = tile.type;
                    }
                });
            }
        });

        // Update wall
        const wallTiles = boardElement.querySelectorAll('.wall .tile');
        board.wall.forEach((row, rowIndex) => {
            row.forEach((spot, colIndex) => {
                const tileIndex = rowIndex * 5 + colIndex;
                if (spot.hasTile) {
                    wallTiles[tileIndex].classList.add('filled');
                    wallTiles[tileIndex].style.backgroundColor = getColorForTile(spot.type);
                    wallTiles[tileIndex].dataset.tileType = spot.type;
                } else {
                    wallTiles[tileIndex].classList.remove('filled');
                    wallTiles[tileIndex].style.backgroundColor = '';
                    wallTiles[tileIndex].dataset.tileType = '';
                }
            });
        });

        // Update floor line
        const floorLineTiles = boardElement.querySelectorAll('.floor-line .tile');
        board.floorLine.forEach((spot, index) => {
            if (spot && spot.hasTile && spot.type) {
                floorLineTiles[index].classList.add('filled');
                floorLineTiles[index].style.backgroundColor = getColorForTile(spot.type);
                floorLineTiles[index].dataset.tileType = spot.type;
                floorLineTiles[index].textContent = spot.type === 10 ? 'Start' : spot.type;
                
                // Special styling for starting tile
                if (spot.type === 10) {
                    floorLineTiles[index].style.fontWeight = 'bold';
                    floorLineTiles[index].style.border = '2px dashed black';
                }
            } else {
                floorLineTiles[index].classList.remove('filled');
                floorLineTiles[index].style.backgroundColor = '';
                floorLineTiles[index].dataset.tileType = '';
                floorLineTiles[index].textContent = '';
                floorLineTiles[index].style.fontWeight = '';
                floorLineTiles[index].style.border = '2px solid #888';
            }
        });
    }
});