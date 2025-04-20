document.getElementById("startGame").addEventListener("click", async function() {
    console.log("Start Game knop is geklikt!");

    const speltype = document.getElementById("speltype").value;
    console.log("Gekozen speltype:", speltype);

    const playerId = "player123";
    console.log("Player ID:", playerId);

    try {
        const response = await fetch("http://localhost:5000/api/Tables/join-or-create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ speltype: parseInt(speltype), playerId })
        });

        console.log("Response ontvangen:", response);

        if (response.ok) {
            const table = await response.json();
            console.log("Tafel data ontvangen:", table);
            document.getElementById("tableInfo").innerText = 
                `Tafel ID: ${table.id} | Spelers: ${table.currentPlayers}/${table.maxPlayers}`;
        } else {
            console.error("Fout bij het verbinden met een tafel.");
            document.getElementById("tableInfo").innerText = "Fout bij verbinden met een tafel.";
        }
    } catch (error) {
        console.error("Netwerkfout:", error);
        document.getElementById("tableInfo").innerText = "Kan geen verbinding maken met de server.";
    }
});