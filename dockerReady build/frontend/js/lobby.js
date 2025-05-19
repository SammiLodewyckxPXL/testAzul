// Initieel: toon enkel de Start knop
document.getElementById("leaveTable").style.display = "none";


let currentTableId = null; // Sla de tafel ID globaal op
let table;
let tableJoined =false;
let token;
document.getElementById("startGame").addEventListener("click", async function() {
    console.log("Start Game knop is geklikt!");

    const speltype = document.getElementById("speltype").value;
    sessionStorage.setItem('aantalSpelers', speltype);

    token = sessionStorage.getItem('authToken');


    try {
        const response = await fetch('http://localhost:5051/api/Tables/join-or-create', {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                'Authorization': "Bearer " + token
            },
            body: JSON.stringify({
                "numberOfPlayers": speltype,
                "numberOfArtificialPlayers": 0
            })
        });

        if (response.ok) {
            table = await response.json();
            console.log("Tafel data ontvangen:", table);

            currentTableId = table.id; // Sla de ID op
            /*document.getElementById("tableInfo").innerText =
                `Tafel ID: ${table.id} | Spelers: ${table.currentPlayers}/${table.maxPlayers}`;*/
            tableJoined = true;
            const totalSeats = table.preferences.numberOfPlayers;
            const occupiedSeats = table.seatedPlayers.length;
            // Tafel info tonen
            document.getElementById("tableInfo").innerText =
                `Tafel ID: ${table.id}\nSpelers: ${occupiedSeats} of ${totalSeats}`;

            document.getElementById("startGame").style.display = "none";
            document.getElementById("leaveTable").style.display = "inline-block";
        } else {
            document.getElementById("tableInfo").innerText = "Fout bij verbinden met een tafel.";
        }
    } catch (error) {
        console.error("Netwerkfout:", error);
        document.getElementById("tableInfo").innerText = "Kan geen verbinding maken met de server.";
    }
});
setInterval(getTableInfo, 1000);
// tableInfo
async function getTableInfo() {
    if (tableJoined) {
        let totalSeats;
        let occupiedSeats;
        try {
            const response = await fetch(`http://localhost:5051/api/Tables/${table.id}`, {
                method: "GET",
                headers: {
                    'Authorization': "Bearer " + token
                }
            });
            if (response.ok) {
                const responseGet = await response.json();
                totalSeats = responseGet.preferences.numberOfPlayers;
                occupiedSeats = responseGet.seatedPlayers.length;
                gameID=
                    document.getElementById("tableInfo").innerText =
                        `Tafel ID: ${table.id}\nSpelers: ${occupiedSeats} of ${totalSeats}`;
            }
            if (totalSeats === occupiedSeats){
                sessionStorage.setItem("tableID", table.id);
                sessionStorage.setItem("gameID", table.gameId);
                window.location.href = "game.html";
            }
        }
        catch (error) {
        }
    }
}

document.getElementById("leaveTable").addEventListener("click", async function () {
    if (!currentTableId) {
        console.warn("Geen tafel ID gevonden om te verlaten.");
        return;
    }

    const token = sessionStorage.getItem('authToken');

    try {
        const response = await fetch(`http://localhost:5051/api/Tables/${currentTableId}/leave`, {
            method: "POST",
            headers: {
                'Authorization': "Bearer " + token
            }
        });

        if (response.ok) {
            console.log("Tafel succesvol verlaten.");
            document.getElementById("tableInfo").innerText = "Je hebt de tafel verlaten.";
            currentTableId = null;

            // UI terug resetten
            document.getElementById("startGame").style.display = "inline-block";
            document.getElementById("leaveTable").style.display = "none";
            tableJoined = false;
        } else {
            const errorText = await response.text();
            alert("Kon de tafel niet verlaten: " + errorText);
        }
    } catch (err) {
        console.error("Netwerkfout bij verlaten van tafel:", err);
    }
});
document.getElementById("loginButton").addEventListener("click", () => {
    window.location.href = "login.html"; // of een andere route naar jouw loginpagina
});
document.getElementById("teamButton").addEventListener("click", () => {
    window.location.href = "team.html"; // of een andere route naar jouw loginpagina
});
window.addEventListener("DOMContentLoaded", () => {
    const loginButton = document.getElementById("loginButton");
    const token = sessionStorage.getItem("authToken");

    if (token) {
        // Verander knop naar "Logout"
        loginButton.textContent = "Logout";
        loginButton.addEventListener("click", () => {
            sessionStorage.removeItem("authToken");
            alert("Je bent uitgelogd.");
            location.reload(); // Pagina herladen om status te resetten
        });
    } else {
        // Als niet ingelogd, ga naar login pagina
        loginButton.addEventListener("click", () => {
            window.location.href = "login.html";
        });
    }
});
