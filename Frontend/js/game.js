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

    let winnerDisplayed = false;
    let placeOnFloorLineBtn = null;
    let errorMessageElement = null;
    let boardOverlays = [];

    function isCurrentPlayerTurn() {
        const currentPlayerId = sessionStorage.getItem("playerId");
        const isTurn = currentPlayerId === gameData.playerToPlayId;
        console.log('Checking if current player turn:', {
            currentPlayerId,
            playerToPlayId: gameData.playerToPlayId,
            isTurn
        });
        return isTurn;
    }

    function updateUIForTurn() {
        const currentPlayerId = sessionStorage.getItem("playerId");
        const isMyTurn = currentPlayerId === gameData.playerToPlayId;

        // Update cursor style for all interactive elements
        document.querySelectorAll('.factory-tile, .pattern-line:not(.floor-line)').forEach(element => {
            if (isMyTurn && !hasSelectedTile) {
                element.style.cursor = 'pointer';
                element.style.opacity = '1';
            } else {
                element.style.cursor = 'not-allowed';
                element.style.opacity = '0.6';
            }
        });

        // Update floor line button if it exists
        if (placeOnFloorLineBtn) {
            if (isMyTurn && hasSelectedTile) {
                placeOnFloorLineBtn.style.cursor = 'pointer';
                placeOnFloorLineBtn.style.opacity = '1';
            } else {
                placeOnFloorLineBtn.style.cursor = 'not-allowed';
                placeOnFloorLineBtn.style.opacity = '0.6';
            }
        }
    }

    function showErrorMessage(message) {
        // Remove existing error message if any
        if (errorMessageElement) {
            errorMessageElement.remove();
        }

        // Create new error message element
        errorMessageElement = document.createElement('div');
        errorMessageElement.style.color = '#dc3545';
        errorMessageElement.style.backgroundColor = '#f8d7da';
        errorMessageElement.style.border = '1px solid #f5c6cb';
        errorMessageElement.style.borderRadius = '4px';
        errorMessageElement.style.padding = '10px';
        errorMessageElement.style.margin = '10px 0';
        errorMessageElement.style.textAlign = 'center';
        errorMessageElement.style.fontWeight = 'bold';
        errorMessageElement.style.position = 'relative';
        errorMessageElement.style.width = '100%';
        errorMessageElement.style.maxWidth = '600px';
        errorMessageElement.style.marginLeft = 'auto';
        errorMessageElement.style.marginRight = 'auto';

        // Add close button
        const closeButton = document.createElement('button');
        closeButton.innerHTML = '&times;';
        closeButton.style.position = 'absolute';
        closeButton.style.right = '10px';
        closeButton.style.top = '50%';
        closeButton.style.transform = 'translateY(-50%)';
        closeButton.style.background = 'none';
        closeButton.style.border = 'none';
        closeButton.style.fontSize = '20px';
        closeButton.style.cursor = 'pointer';
        closeButton.style.color = '#dc3545';
        closeButton.onclick = () => errorMessageElement.remove();

        errorMessageElement.appendChild(document.createTextNode(message));
        errorMessageElement.appendChild(closeButton);

        // Insert error message above the factory displays
        const tableCenter = document.getElementById('tableCenter');
        if (tableCenter && tableCenter.parentNode) {
            tableCenter.parentNode.insertBefore(errorMessageElement, tableCenter);
        }
    }

    function clearErrorMessage() {
        if (errorMessageElement) {
            errorMessageElement.remove();
            errorMessageElement = null;
        }
    }

    function updateBoardInteractivity() {
        const currentPlayerId = sessionStorage.getItem("playerId");
        const boards = Array.from(document.querySelectorAll('.azul-board'));

        console.log('Updating board interactivity:', {
            currentPlayerId,
            playerToPlayId: gameData.playerToPlayId,
            players: gameData.players.map(p => ({ id: p.id, name: p.name }))
        });

        boards.forEach((board, index) => {
            const playerId = gameData.players[index]?.id;
            const isCurrentPlayerBoard = playerId === currentPlayerId;
            const isPlayerTurn = currentPlayerId === gameData.playerToPlayId;

            console.log(`Board ${index + 1} interactivity:`, {
                index,
                playerId,
                isCurrentPlayerBoard,
                isPlayerTurn
            });

            // Reset all pattern lines first
            const patternLines = board.querySelectorAll(".pattern-line:not(.floor-line)");
            patternLines.forEach(line => {
                // Remove any existing event listeners by cloning
                const newLine = line.cloneNode(true);
                line.parentNode.replaceChild(newLine, line);
                
                // Set default styles
                newLine.style.cursor = 'default';
                newLine.style.pointerEvents = 'none';
            });

            // Make current player's board interactive if it's their turn
            if (isCurrentPlayerBoard && isPlayerTurn) {
                console.log(`Making board ${index + 1} interactive`);
                patternLines.forEach(line => {
                    line.style.cursor = 'pointer';
                    line.style.pointerEvents = 'auto';
                });
            }
        });
    }

    async function getGameData() {
        try {
            const response = await fetch(`/api/games/${tableData.gameId}`, {
                method: "GET",
                headers: {
                    'Authorization': "Bearer " + token
                }
            });
            if (response.ok) {
                gameData = await response.json();
                console.log("Game data received:", {
                    fullData: gameData,
                    players: gameData.players.map(p => ({
                        id: p.id,
                        floorLine: p.board.floorLine,
                        patternLines: p.board.patternLines
                    }))
                });

                // Remove the floor line button if no tiles are selected
                if (!hasSelectedTile) {
                    const existingButton = document.getElementById('placeOnFloorLineBtn');
                    if (existingButton) {
                        existingButton.remove();
                    }
                }

                updatePlayerHighlights();
                updatePlayerScore();
                if (gameData && gameData.tileFactory) {
                    loadFactoryTiles(gameData.tileFactory);
                }
                updateTableCenter();
                updatePatternLines(gameData);
                updateFloorLines(gameData);
                updateWall(gameData);
                showWinner(gameData);
                updateUIForTurn();
                clearErrorMessage();
                updateBoardInteractivity();
            } else {
                console.error("Error fetching game data:", await response.text());
            }
        } catch (error) {
            console.error("Error fetching game data:", error);
        }
    }

    try {
        const response = await fetch(`/api/Tables/${tableId}`, {
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

                    if (isCurrentPlayerTurn() && !hasSelectedTile) {
                        tileElement.style.cursor = "pointer";
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
                                const response = await fetch(`/api/games/${tableData.gameId}/take-tiles`, {
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
            case 11: return "#4160f9";  // Blue
            case 12: return "#f5c305";  // Yellow
            case 13: return "#ff0000";  // Red
            case 14: return "#000000";  // Black
            case 15: return "#30d5c8";  // Turquoise (lowercase for consistency)
            default: return "#adb5bd";
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

        gameData.players.forEach((player, index) => {
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
        gameData.players.forEach((player, index) => {
            const boardNumber = index + 1;
            const boardElement = document.getElementById(`board${boardNumber}`);

            if (boardElement) {
                let scoreLabel = document.getElementById(`playerScore${boardNumber}`);
                if (!scoreLabel) {
                    scoreLabel = document.createElement("div");
                    scoreLabel.id = `playerScore${boardNumber}`;
                    scoreLabel.style.textAlign = "center";
                    scoreLabel.style.fontWeight = "bold";
                    scoreLabel.style.marginBottom = "4px";
                    boardElement.parentNode.insertBefore(scoreLabel, boardElement);
                }

                scoreLabel.textContent = "Score: " + player.board.score;
            }
        });
    }

    function updatePatternLines(gameData) {
        if (!gameData || !gameData.players) {
            console.warn("No game data or players available");
            return;
        }

        const boards = document.querySelectorAll('.azul-board');

        gameData.players.forEach((player, index) => {
            if (!player || !player.board || !player.board.patternLines) {
                console.warn(`Invalid player data for index ${index}`);
                return;
            }

            // Make sure we have a valid board for this player
            const board = boards[index];
            if (!board) {
                console.warn(`Board element not found for player ${index}`);
                return;
            }

            // Update each pattern line
            player.board.patternLines.forEach((patternLine, backendIndex) => {
                if (!patternLine) {
                    console.warn(`Invalid pattern line data for line ${backendIndex}`);
                    return;
                }

                // Convert backend index to frontend index (reverse the order)
                const frontendIndex = 4 - backendIndex;

                const patternLineElement = board.querySelector(`.pattern-line[data-index="${frontendIndex}"]`);
                if (!patternLineElement) {
                    console.warn(`Pattern line element not found for index ${frontendIndex}`);
                    return;
                }

                const tiles = Array.from(patternLineElement.querySelectorAll('.tile'));
                if (!tiles || tiles.length === 0) {
                    console.warn(`No tiles found in pattern line ${frontendIndex}`);
                    return;
                }

                // Reset all tiles first
                tiles.forEach(tile => {
                    if (tile && tile.classList) {
                        tile.className = 'tile';
                        tile.style.backgroundColor = '';
                        tile.textContent = '';
                        if (tile.dataset) {
                            delete tile.dataset.tileType;
                        }
                    }
                });

                // Fill pattern line with tiles from the data
                if (patternLine.numberOfTiles > 0 && patternLine.tileType) {
                    const startIndex = tiles.length - patternLine.numberOfTiles;
                    for (let i = startIndex; i < tiles.length; i++) {
                        const tile = tiles[i];
                        if (tile && tile.classList) {
                            tile.classList.add('filled');
                            tile.style.backgroundColor = getColorForTile(patternLine.tileType);
                            tile.textContent = patternLine.tileType;
                            if (tile.dataset) {
                                tile.dataset.tileType = patternLine.tileType;
                            }
                        }
                    }
                }
            });
        });
    }

    function enablePatternLineSelection(gameId, selectedTileColor, aantalTegels) {
        // Remove any existing floor line button
        const existingButton = document.getElementById('placeOnFloorLineBtn');
        if (existingButton) {
            existingButton.remove();
        }

        if (!hasSelectedTile || !isCurrentPlayerTurn()) {
            console.log('Pattern line selection disabled:', {
                hasSelectedTile,
                isCurrentPlayerTurn: isCurrentPlayerTurn()
            });
            return;
        }

        console.log('Enabling pattern line selection');

        // Create floor line button
        placeOnFloorLineBtn = document.createElement('button');
        placeOnFloorLineBtn.id = 'placeOnFloorLineBtn';
        placeOnFloorLineBtn.textContent = 'Plaats tegels op vloerlijn';
        placeOnFloorLineBtn.style.display = 'block';
        placeOnFloorLineBtn.style.margin = '10px auto';
        placeOnFloorLineBtn.style.padding = '8px 16px';
        placeOnFloorLineBtn.style.backgroundColor = '#dc3545';
        placeOnFloorLineBtn.style.color = 'white';
        placeOnFloorLineBtn.style.border = 'none';
        placeOnFloorLineBtn.style.borderRadius = '4px';
        placeOnFloorLineBtn.style.cursor = 'pointer';

        // Add hover effect
        placeOnFloorLineBtn.addEventListener('mouseover', () => {
            placeOnFloorLineBtn.style.backgroundColor = '#c82333';
        });
        placeOnFloorLineBtn.addEventListener('mouseout', () => {
            placeOnFloorLineBtn.style.backgroundColor = '#dc3545';
        });

        const tableCenter = document.getElementById('tableCenter');
        tableCenter.parentNode.insertBefore(placeOnFloorLineBtn, tableCenter.nextSibling);

        placeOnFloorLineBtn.addEventListener('click', async () => {
            if (!hasSelectedTile) {
                placeOnFloorLineBtn.remove();
                return;
            }

            try {
                const response = await fetch(`/api/Games/${gameId}/place-tiles-on-floorline`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + token
                    },
                    body: JSON.stringify({})
                });

                if (response.ok) {
                    console.log('Tiles placed on floor line');
                    hasSelectedTile = false;
                    placeOnFloorLineBtn.remove();
                    placeOnFloorLineBtn = null;

                    // Update game data and UI
                    await getGameData();
                    updatePlayerHighlights();

                    // Restart auto refresh
                    autoRefreshInterval = setInterval(getGameData, 1000);
                } else {
                    const errorText = await response.text();
                    console.error('Server error:', errorText);
                    showErrorMessage('Fout bij plaatsen van tegel op vloerlijn: ' + errorText);
                    hasSelectedTile = false;
                    placeOnFloorLineBtn.remove();
                    placeOnFloorLineBtn = null;
                }
            } catch (err) {
                console.error('Netwerkfout:', err);
                showErrorMessage('Kon geen verbinding maken met server.');
                hasSelectedTile = false;
                placeOnFloorLineBtn.remove();
                placeOnFloorLineBtn = null;
            }
        });

        // Pattern line selection code
        const currentPlayerId = sessionStorage.getItem("playerId");
        const boards = Array.from(document.querySelectorAll('.azul-board'));
        
        // Find the current player's board
        const currentPlayerBoard = boards.find((board, index) => {
            const playerId = gameData.players[index]?.id;
            
            console.log(`Checking board ${index + 1}:`, {
                index,
                playerId,
                currentPlayerId,
                isCurrentBoard: playerId === currentPlayerId,
                players: gameData.players.map(p => ({ id: p.id, name: p.name }))
            });
            
            return playerId === currentPlayerId;
        });

        if (currentPlayerBoard) {
            console.log('Found current player board, adding click handlers');
            const patternLines = currentPlayerBoard.querySelectorAll(".pattern-line:not(.floor-line)");
            patternLines.forEach((line) => {
                line.style.cursor = 'pointer';
                line.style.pointerEvents = 'auto';
                line.addEventListener("click", async () => {
                    console.log('Pattern line clicked:', {
                        hasSelectedTile,
                        isCurrentPlayerTurn: isCurrentPlayerTurn()
                    });
                    if (!hasSelectedTile || !isCurrentPlayerTurn()) return;

                    // Get the frontend index from the HTML
                    const frontendIndex = parseInt(line.dataset.index);
                    // Convert to backend index
                    const patternLineIndex = 4 - frontendIndex;

                    // Get tile type from color
                    let selectedTileType = null;
                    // Normalize the color by removing spaces and converting to lowercase
                    const normalizedSelectedColor = selectedTileColor.toLowerCase().replace(/\s/g, '');
                    
                    // Create a map of normalized colors to tile types
                    const colorToTileType = {
                        '#4160f9': 11,  // Blue
                        '#f5c305': 12,  // Yellow
                        '#ff0000': 13,  // Red
                        '#000000': 14,  // Black
                        '#30d5c8': 15   // Turquoise
                    };

                    selectedTileType = colorToTileType[normalizedSelectedColor];

                    if (!selectedTileType) {
                        console.error('Could not determine tile type from color:', selectedTileColor);
                        return;
                    }

                    // Check if this color already exists in the wall row
                    const currentPlayer = gameData.players.find(p => p.id === currentPlayerId);
                    if (currentPlayer) {
                        const wallRow = currentPlayer.board.wall[patternLineIndex];
                        
                        const hasTileInWall = wallRow.some(spot => 
                            spot.hasTile && spot.type === selectedTileType
                        );

                        if (hasTileInWall) {
                            showErrorMessage('Je kunt geen tegel van deze kleur op deze rij plaatsen omdat er al een tegel van dezelfde kleur op de muur staat.');
                            return;
                        }
                    }

                    const tiles = line.querySelectorAll('.tile');

                    // Count already filled tiles and check existing color
                    let alreadyFilled = 0;
                    let existingColor = null;
                    let existingTileType = null;
                    tiles.forEach(tile => {
                        if (tile.classList.contains('filled')) {
                            alreadyFilled++;
                            if (!existingColor) {
                                existingColor = tile.style.backgroundColor.toLowerCase().trim();
                                existingTileType = tile.dataset.tileType || tile.textContent;
                            }
                        }
                    });

                    // Check for color conflict
                    if (existingColor && existingColor !== selectedTileColor.toLowerCase().trim()) {
                        // Double check with the actual color values
                        const normalizedExisting = existingColor.replace(/\s/g, '');
                        const normalizedSelected = selectedTileColor.toLowerCase().replace(/\s/g, '');

                        // Also check by tile type
                        const sameType = existingTileType && selectedTileType &&
                            parseInt(existingTileType) === selectedTileType;

                        if (normalizedExisting !== normalizedSelected && !sameType) {
                            showErrorMessage("Je kunt geen tegels van verschillende kleuren op dezelfde patroonlijn plaatsen.");
                            return;
                        }
                    }

                    // Check if line is full
                    const maxCapacity = tiles.length;
                    if (alreadyFilled === maxCapacity) {
                        showErrorMessage("Deze patroonlijn is al volledig gevuld.");
                        return;
                    }

                    try {
                        // Place tiles on pattern line
                        const response = await fetch(`/api/games/${gameId}/place-tiles-on-patternline`, {
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
                            console.log(`Tiles placed on pattern line ${patternLineIndex}`);
                            hasSelectedTile = false;

                            // Update game data and UI
                            await getGameData();
                            updatePlayerHighlights();
                            updatePlayerScore();

                            // Restart auto refresh
                            autoRefreshInterval = setInterval(getGameData, 1000);
                        } else {
                            const errorText = await response.text();
                            console.error("Server error:", errorText);
                            showErrorMessage("Fout bij plaatsen van tegel: " + errorText);

                            // Reset selection state on error
                            hasSelectedTile = false;
                        }
                    } catch (err) {
                        console.error("Netwerkfout:", err);
                        showErrorMessage("Kon geen verbinding maken met server.");
                    }
                });
            });
        } else {
            console.log('Current player board not found');
        }
    }

    function updateFloorLines(gameData) {
        console.log('Updating floor lines with game data:', gameData);

        gameData.players.forEach((player, index) => {
            console.log(`Updating floor line for player ${index}:`, player.board.floorLine);

            const board = document.querySelectorAll('.azul-board')[index];
            const floorLine = board.querySelector('.floor-line');

            if (floorLine && player.board.floorLine) {
                const floorTiles = floorLine.querySelectorAll('.tile');
                console.log('Found floor tiles:', floorTiles.length);

                // Clear floor line
                floorTiles.forEach(tile => {
                    tile.className = 'tile';
                    tile.style.backgroundColor = '';
                    tile.textContent = '';
                });

                // Fill floor line with tiles from the data
                player.board.floorLine.forEach((spot, i) => {
                    console.log(`Processing floor spot ${i}:`, spot);
                    if (spot.hasTile && floorTiles[i]) {
                        floorTiles[i].classList.add('filled');
                        if (spot.type === 10) { // 10 = starttegel (StartingTile)
                            floorTiles[i].style.backgroundColor = '#adb5bd';
                            floorTiles[i].textContent = 'Start';
                            floorTiles[i].style.fontWeight = 'bold';
                            floorTiles[i].style.border = '2px dashed black';
                        } else {
                            floorTiles[i].style.backgroundColor = getColorForTile(spot.type);
                            floorTiles[i].textContent = spot.type;
                        }
                    }
                });
            } else {
                console.warn('Floor line element or data not found:', {
                    floorLineElement: !!floorLine,
                    floorLineData: !!player.board.floorLine
                });
            }
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

        // Controleer of de starttegel aanwezig is
        hasStartTile = centerTiles.includes(10); // Aanname: 10 = starttegel
        console.log(`Tegels in het midden: ${centerTiles.length}`, centerTiles);

        // Maak eerst de container voor het aantal geselecteerde tegels
        const selectedCountContainer = document.createElement("div");
        selectedCountContainer.id = "selectedTilesInfo";
        selectedCountContainer.style.textAlign = "center";
        selectedCountContainer.style.fontWeight = "bold";
        selectedCountContainer.style.marginTop = "8px";
        selectedCountContainer.style.marginBottom = "8px";
        selectedCountContainer.style.display = "none";
        tableCenter.appendChild(selectedCountContainer);

        // Maak een object om het aantal tegels per kleur bij te houden
        const tilesByType = {};
        centerTiles.forEach(tile => {
            if (tile !== 10) { // Negeer starttegel bij tellen
                if (!tilesByType[tile]) {
                    tilesByType[tile] = 1;
                } else {
                    tilesByType[tile]++;
                }
            }
        });

        // Voeg de starttegel toe als die er is
        if (hasStartTile) {
            const startTileElement = document.createElement("div");
            startTileElement.className = "factory-tile";
            startTileElement.textContent = "Start";
            startTileElement.style.backgroundColor = "#adb5bd";
            startTileElement.style.border = "2px dashed black";
            startTileElement.style.fontWeight = "bold";

            // Voeg een tooltip toe om uitleg te geven
            startTileElement.title = "Starttegel: De eerste speler die tegels uit het midden pakt, ontvangt deze tegel.";

            tableCenter.appendChild(startTileElement);
        }

        // Voeg alle tegels per kleur toe
        Object.keys(tilesByType).forEach(tileType => {
            const count = tilesByType[tileType];
            const color = getColorForTile(parseInt(tileType));

            // Groepeer tegels van dezelfde kleur bij elkaar
            const tileGroupContainer = document.createElement("div");
            tileGroupContainer.className = "tile-group";
            tileGroupContainer.style.display = "inline-block";
            tileGroupContainer.style.margin = "5px";
            tileGroupContainer.style.position = "relative";

            // Voeg het aantal tegels toe als badge
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

            // Maak de tegel zelf
            const tileElement = document.createElement("div");
            tileElement.className = "factory-tile";
            tileElement.textContent = tileType;
            tileElement.style.backgroundColor = color;

            // Alleen click event toevoegen als het de beurt van de huidige speler is
            if (isCurrentPlayerTurn() && !hasSelectedTile) {
                tileElement.style.cursor = "pointer";
                tileElement.addEventListener('click', async () => {
                    if (hasSelectedTile) return;
                    hasSelectedTile = true;

                    // Pauzeer automatische vernieuwing
                    clearInterval(autoRefreshInterval);

                    // Markeer de geselecteerde tegels
                    tileElement.style.border = "3px solid green";

                    // Toon aantal geselecteerde tegels
                    selectedCountContainer.style.display = "block";
                    selectedCountContainer.textContent = `Geselecteerd: ${count} ${color} tegel(s)${hasStartTile ? " + starttegel" : ""}`;
                    if (hasStartTile) {
                        selectedCountContainer.innerHTML += `<br><span style="color: #333; font-style: italic;">(Je bent de eerste die tegels uit het midden pakt)</span>`;
                    }
                    selectedCountContainer.style.color = color;

                    try {
                        const response = await fetch(`/api/games/${gameData.id}/take-tiles`, {
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
                            // Hervat automatische vernieuwing bij fout
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
                        // Hervat automatische vernieuwing bij fout
                        autoRefreshInterval = setInterval(getGameData, 1000);
                    }
                });
            }

            tileGroupContainer.appendChild(tileElement);
            tileGroupContainer.appendChild(countBadge);
            tableCenter.appendChild(tileGroupContainer);
        });
    }

    function updateWall(gameData) {
        if (!gameData || !gameData.players) return;

        gameData.players.forEach((player, playerIndex) => {
            const board = document.querySelectorAll('.azul-board')[playerIndex];
            if (!board) return;

            const wallTiles = board.querySelectorAll('.wall .tile');
            if (!wallTiles || !player.board.wall) return;

            // Loop through each row and column of the wall
            player.board.wall.forEach((row, rowIndex) => {
                row.forEach((wallSpot, colIndex) => {
                    const tileIndex = rowIndex * 5 + colIndex;
                    const tile = wallTiles[tileIndex];

                    if (wallSpot.hasTile) {
                        // Remove the striped background by removing the tileXX class
                        tile.className = 'tile filled';
                        tile.style.backgroundColor = getColorForTile(wallSpot.type);
                        tile.style.border = `3px solid ${getColorForTile(wallSpot.type)}`;
                        tile.textContent = wallSpot.type;
                    } else {
                        // Reset to the original striped pattern by keeping only the tileXX class
                        const tileTypeClass = Array.from(tile.classList).find(c => c.startsWith('tile1'));
                        tile.className = `tile ${tileTypeClass || ''}`;
                        tile.style.backgroundColor = '';
                        tile.textContent = '';
                    }
                });
            });
        });

        // Update scores after wall changes
        updatePlayerScore();
    }

    function showWinner(gameData) {
        if (!gameData.hasEnded || winnerDisplayed) return;

        // Find the player with the highest score
        const winner = gameData.players.reduce((highest, current) => {
            return (current.board.score > highest.board.score) ? current : highest;
        }, gameData.players[0]);

        // Create winner announcement container
        const winnerContainer = document.createElement('div');
        winnerContainer.style.position = 'fixed';
        winnerContainer.style.top = '50%';
        winnerContainer.style.left = '50%';
        winnerContainer.style.transform = 'translate(-50%, -50%)';
        winnerContainer.style.backgroundColor = '#4CAF50';
        winnerContainer.style.padding = '20px';
        winnerContainer.style.borderRadius = '10px';
        winnerContainer.style.boxShadow = '0 4px 8px rgba(0,0,0,0.2)';
        winnerContainer.style.zIndex = '1000';
        winnerContainer.style.textAlign = 'center';
        winnerContainer.style.color = 'white';
        winnerContainer.style.fontSize = '24px';
        winnerContainer.style.fontWeight = 'bold';

        // Add confetti animation class
        winnerContainer.className = 'winner-announcement';

        // Create winner text
        const winnerText = document.createElement('div');
        winnerText.innerHTML = `🎉 ${winner.name} heeft gewonnen! 🎉<br>Score: ${winner.board.score}`;
        winnerContainer.appendChild(winnerText);

        // Add scores of all players
        const scoresContainer = document.createElement('div');
        scoresContainer.style.marginTop = '20px';
        scoresContainer.style.fontSize = '18px';
        scoresContainer.innerHTML = '<strong>Eindscores:</strong><br>' + 
            gameData.players
                .sort((a, b) => b.board.score - a.board.score)
                .map(player => `${player.name}: ${player.board.score} punten`)
                .join('<br>');
        winnerContainer.appendChild(scoresContainer);

        // Add a close button
        const closeButton = document.createElement('button');
        closeButton.textContent = 'Sluiten';
        closeButton.style.marginTop = '20px';
        closeButton.style.padding = '10px 20px';
        closeButton.style.border = 'none';
        closeButton.style.borderRadius = '5px';
        closeButton.style.backgroundColor = 'white';
        closeButton.style.color = '#4CAF50';
        closeButton.style.cursor = 'pointer';
        closeButton.style.fontWeight = 'bold';
        closeButton.onclick = () => {
            winnerContainer.remove();
        };
        winnerContainer.appendChild(closeButton);

        // Add CSS animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes confetti {
                0% { transform: translate(-50%, -50%) rotate(0deg); }
                100% { transform: translate(-50%, -50%) rotate(360deg); }
            }
            .winner-announcement::before {
                content: '🎊';
                position: absolute;
                font-size: 40px;
                animation: confetti 2s linear infinite;
                left: 10px;
                top: 50%;
            }
            .winner-announcement::after {
                content: '🎊';
                position: absolute;
                font-size: 40px;
                animation: confetti 2s linear infinite reverse;
                right: 10px;
                top: 50%;
            }
        `;
        document.head.appendChild(style);

        document.body.appendChild(winnerContainer);
        winnerDisplayed = true;
    }
});