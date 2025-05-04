document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    const email = urlParams.get("email"); // Haal het e-mailadres op
    let foutmelding = document.getElementById("foutmelding");
    foutmelding.innerText="";
    // Als er een e-mailadres in de URL zit, vul het in het e-mailveld in
    if (email) {
        document.getElementById("email").value = email;
    }
    document.getElementById("loginForm").addEventListener("submit", async function (event) {
        event.preventDefault();

        let email = document.getElementById("email").value;
        let password = document.getElementById("password").value;

        if (!email || !password) {
            foutmelding.innerText="Vul zowel je e-mail als wachtwoord in.";
            return;
        }

        let requestBody = {
            email: email,
            password: password
        };

        try {
            let response = await fetch("https://localhost:5051/api/Authentication/token", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(requestBody)
            });

            let responseText = await response.text();
            let data = responseText ? JSON.parse(responseText) : {};

            if (!response.ok) {
                foutmelding.innerText=data.message || "Ongeldige inloggegevens.";
                return;
            }

            // Sla het token op en navigeer naar de lobby
            sessionStorage.setItem("authToken", data.token);
            foutmelding.innerText="Inloggen succesvol! Je wordt doorgestuurd naar de lobby.";
            window.location.href = "lobby.html";
        } catch (error) {
            foutmelding.innerText=error.toString();
        }
    });
});
