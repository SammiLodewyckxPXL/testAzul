document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("registerForm").addEventListener("submit", async function (event) {
        event.preventDefault();

        let email = document.getElementById("email").value;
        let username = document.getElementById("username").value;
        let password = document.getElementById("password").value;
        let confirmPassword = document.getElementById("confirmPassword").value;
        let lastVisitToPortugal = document.getElementById("lastStayDate").value;
        let foutmelding = document.getElementById("foutmelding");

        // Verwijder eerdere foutmeldingen
        while (foutmelding.firstChild) {
            foutmelding.removeChild(foutmelding.firstChild);
        }

        if (!email || !username || !password || !confirmPassword) {
            toonFoutmelding("Alle verplichte velden moeten ingevuld zijn.");
            return;
        }

        if (password.length < 6) {
            toonFoutmelding("Het wachtwoord moet minstens 6 karakters bevatten.");
            return;
        }

        if (password !== confirmPassword) {
            toonFoutmelding("De wachtwoorden komen niet overeen.");
            return;
        }

        if (lastVisitToPortugal) {
            let stayDate = new Date(lastVisitToPortugal);
            let today = new Date();
            today.setHours(0, 0, 0, 0);

            if (stayDate > today) {
                toonFoutmelding("De verblijfsdatum in Portugal moet in het verleden liggen.");
                return;
            }
        }

        let requestBody = {
            email: email,
            username: username,
            password: password,
            lastVisitToPortugal: lastVisitToPortugal || null
        };

        try {
            let response = await fetch("http://localhost:5051/api/Authentication/register", {
                method: "POST",
                headers: { 
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                credentials: 'include',
                body: JSON.stringify(requestBody)
            });

            let responseText = await response.text();
            let data = responseText ? JSON.parse(responseText) : {};

            if (!response.ok) {
                toonFoutmelding(data.message || "Er is een fout opgetreden bij het registreren.");
                return;
            }

            window.location.href = "login.html?email=" + encodeURIComponent(email);
        } catch (error) {
            toonFoutmelding(error.toString());
        }
    });

    function toonFoutmelding(tekst) {
        const p = document.createElement("p");
        const tekstNode = document.createTextNode(tekst);
        p.appendChild(tekstNode);
        p.style.color = "red";
        p.style.margin = "4px 0";
        document.getElementById("foutmelding").appendChild(p);
    }
});
