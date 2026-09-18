(function () {
    const token = getToken();
    if (!token) {
        window.location.href = "login.html";
        return;
    }

    const requiredRole = document.body.dataset.requiredRole;
    const roles = getRoles();
    const isAllowed = roles.includes("Admin") || (requiredRole && roles.includes(requiredRole));

    if (requiredRole && !isAllowed) {
        window.location.href = "login.html";
        return;
    }

    const user = getUser();
    const emailEl = document.getElementById("topbar-email");
    const roleEl = document.getElementById("topbar-role");
    if (emailEl) emailEl.textContent = (user && user.email) || "Unknown user";
    if (roleEl) roleEl.textContent = roles.join(", ") || "No role";

    const logoutBtn = document.getElementById("logout-btn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", function () {
            localStorage.removeItem("token");
            localStorage.removeItem("user");
            localStorage.removeItem("roles");
            window.location.href = "login.html";
        });
    }
})();
