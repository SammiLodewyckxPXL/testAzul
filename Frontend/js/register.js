document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("registerForm").addEventListener("submit", async function (event) {
        event.preventDefault();

        let email = document.getElementById("email").value;
        let username = document.getElementById("username").value;
        let password = document.getElementById("password").value;
        let confirmPassword = document.getElementById("confirmPassword").value;
        let lastVisitToPortugal = document.getElementById("lastStayDate").value;
        let foutmelding = document.getElementById("foutmelding");
        foutmelding.innerText="";
        if (!email || !username || !password || !confirmPassword) {
            foutmelding.innerText="Alle verplichte velden moeten ingevuld zijn."
            return;
        }

        if (password.length < 6) {
            foutmelding.innerText="Het wachtwoord moet minstens 6 karakters bevatten.";
            return;
        }

        if (password !== confirmPassword) {
            foutmelding.innerText="De wachtwoorden komen niet overeen.";
            return;
        }

        if (lastVisitToPortugal) {
            let stayDate = new Date(lastVisitToPortugal);
            let today = new Date();
            today.setHours(0, 0, 0, 0);

            if (stayDate > today) {
                foutmelding.innerText="De verblijfsdatum in Portugal moet in het verleden liggen.";
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
            let response = await fetch("https://localhost:5051/api/Authentication/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(requestBody)
            });

            let responseText = await response.text();
            let data = responseText ? JSON.parse(responseText) : {};

            if (!response.ok) {
                foutmelding.innerText=data.message || "Er is een fout opgetreden bij het registreren.";
                return;
            }
            window.location.href = "login.html?email=" + encodeURIComponent(email);
        } catch (error) {
            foutmelding.innerText=error.toString();
        }
    });
});
