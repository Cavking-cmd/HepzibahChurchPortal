const ROLE_PRIORITY = ["Admin", "AttendanceOfficer", "FellowshipLeader", "InventoryOfficer", "Viewer"];
const ROLE_DASHBOARD = {
    Admin: "admin.html",
    AttendanceOfficer: "attendance-officer.html",
    FellowshipLeader: "fellowship-leader.html",
    InventoryOfficer: "inventory-officer.html",
    Viewer: "viewer.html"
};

document.querySelector(".login-form").addEventListener("submit", async function (e) {
    e.preventDefault();
    const button = document.querySelector("button[type='submit']");
    const messageEl = document.getElementById("message");
    button.disabled = true;
    messageEl.textContent = "";
    messageEl.className = "";

    const email = document.getElementById("Email").value;
    const password = document.getElementById("Password").value;

    try {
        const { ok, body } = await apiRequest("POST", "/api/User/login", { email, password });

        if (ok && body && body.token) {
            localStorage.setItem("token", body.token);
            localStorage.setItem("user", JSON.stringify(body.user));
            const roles = (body.user && body.user.userRoles) || [];
            localStorage.setItem("roles", JSON.stringify(roles));

            const primaryRole = ROLE_PRIORITY.find(r => roles.includes(r));
            const destination = ROLE_DASHBOARD[primaryRole] || "viewer.html";

            messageEl.classList.add("success");
            messageEl.textContent = "Login successful! Redirecting...";
            window.location.href = destination;
        } else {
            messageEl.classList.add("error");
            messageEl.textContent = "Login failed." + (body && body.message ? " " + body.message : " Please check your credentials.");
        }
    } catch (error) {
        console.error("Error:", error);
        messageEl.classList.add("error");
        messageEl.textContent = "An error occurred. Please try again.";
    } finally {
        button.disabled = false;
    }
});
