document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    const email = urlParams.get("email"); // Haal het e-mailadres op
    let foutmelding = document.getElementById("foutmelding");

    // Verwijder alle kindelementen (foutmeldingen)
    while (foutmelding.firstChild) {
        foutmelding.removeChild(foutmelding.firstChild);
    }

    if (email) {
        document.getElementById("email").value = email;
    }

    document.getElementById("loginForm").addEventListener("submit", async function (event) {
        event.preventDefault();

        // Verwijder bestaande foutmeldingen
        while (foutmelding.firstChild) {
            foutmelding.removeChild(foutmelding.firstChild);
        }

        let email = document.getElementById("email").value;
        let password = document.getElementById("password").value;

        if (!email || !password) {
            toonFoutmelding("Vul zowel je e-mail als wachtwoord in.");
            return;
        }

        let requestBody = {
            email: email,
            password: password
        };

        try {
            let response = await fetch("/api/Authentication/token", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(requestBody)
            });

            let responseText = await response.text();
            let data = responseText ? JSON.parse(responseText) : {};
            console.log("Login response data:", data);


            if (!response.ok) {
                toonFoutmelding(data.message || "Ongeldige inloggegevens.");
                return;
            }

            sessionStorage.setItem("authToken", data.token);
            sessionStorage.setItem("playerId", data.user.id)
            toonFoutmelding("Inloggen succesvol! Je wordt doorgestuurd naar de lobby.", true);
           window.location.href = "lobby.html";
        } catch (error) {
            toonFoutmelding(error.toString());
        }
    });

    function toonFoutmelding(tekst, isSucces = false) {
        const p = document.createElement("p");
        const tekstNode = document.createTextNode(tekst);
        p.appendChild(tekstNode);
        p.style.color = isSucces ? "green" : "red";
        foutmelding.appendChild(p);
    }
});
