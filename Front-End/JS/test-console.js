const apiBaseInput = document.getElementById("api-base");
apiBaseInput.value = localStorage.getItem("apiBase") || apiBaseInput.value;
apiBaseInput.addEventListener("change", () => localStorage.setItem("apiBase", apiBaseInput.value));

function apiBase() {
    return apiBaseInput.value.replace(/\/$/, "");
}

function getToken() {
    return localStorage.getItem("token") || "";
}

function logRequest(method, path, body, response, data) {
    const el = document.getElementById("log-output");
    const entry =
        `${new Date().toLocaleTimeString()}  ${method} ${path}\n` +
        (body ? `  Request body: ${JSON.stringify(body)}\n` : "") +
        `  Status: ${response.status}\n` +
        `  Response: ${JSON.stringify(data, null, 2)}\n` +
        "----------------------------------------\n";
    el.textContent = entry + el.textContent;
}

async function apiRequest(method, path, body) {
    const headers = { "Content-Type": "application/json" };
    const token = getToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const response = await fetch(`${apiBase()}${path}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined
    });

    let data = null;
    const text = await response.text();
    if (text) {
        try { data = JSON.parse(text); } catch { data = text; }
    }

    logRequest(method, path, body, response, data);
    return { ok: response.ok, status: response.status, data };
}

function unwrap(value) {
    if (value && typeof value === "object" && Array.isArray(value.$values)) {
        return value.$values;
    }
    return value ?? [];
}

function updateAuthStatus() {
    const statusEl = document.getElementById("auth-status");
    const token = getToken();
    const userRaw = localStorage.getItem("user");
    if (token && userRaw) {
        const user = JSON.parse(userRaw);
        statusEl.classList.add("logged-in");
        statusEl.textContent = `Logged in as ${user.email} - roles: ${(user.userRoles || []).join(", ")}`;
    } else {
        statusEl.classList.remove("logged-in");
        statusEl.textContent = "Not logged in.";
    }
}

document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const email = document.getElementById("login-email").value;
    const password = document.getElementById("login-password").value;
    const { ok, data } = await apiRequest("POST", "/api/User/login", { email, password });
    if (ok && data && data.token) {
        localStorage.setItem("token", data.token);
        localStorage.setItem("user", JSON.stringify(data.user));
        updateAuthStatus();
        refreshAll();
    } else {
        alert("Login failed - check the log panel below for details.");
    }
});

document.getElementById("logout-btn").addEventListener("click", () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    updateAuthStatus();
});

document.querySelectorAll(".tab-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
        document.querySelectorAll(".tab-btn").forEach((b) => b.classList.remove("active"));
        document.querySelectorAll(".tab-panel").forEach((p) => p.classList.remove("active"));
        btn.classList.add("active");
        document.getElementById(`tab-${btn.dataset.tab}`).classList.add("active");
    });
});

function renderTable(tableId, columns, rows, actions) {
    const table = document.getElementById(tableId);
    const thead = table.querySelector("thead");
    const tbody = table.querySelector("tbody");

    thead.innerHTML = `<tr>${columns.map((c) => `<th>${c.label}</th>`).join("")}<th>Actions</th></tr>`;

    if (!rows.length) {
        tbody.innerHTML = `<tr><td colspan="${columns.length + 1}">No records yet.</td></tr>`;
        return;
    }

    tbody.innerHTML = rows.map((row) => {
        const cells = columns.map((c) => `<td>${c.render ? c.render(row) : (row[c.key] ?? "")}</td>`).join("");
        const actionButtons = actions ? actions(row) : "";
        return `<tr>${cells}<td>${actionButtons}</td></tr>`;
    }).join("");
}

const SERVICE_TYPE_NAMES = ["Sunday", "Wednesday", "Special"];
const ITEM_CONDITION_NAMES = ["Good", "NeedsRepair", "Replace"];

async function loadServices() {
    const { ok, data } = await apiRequest("GET", "/api/Service");
    const rows = ok ? unwrap(data.data) : [];

    renderTable("services-table",
        [
            { label: "Date", render: (r) => (r.date || "").slice(0, 10) },
            { label: "Day", key: "day" },
            { label: "Type", render: (r) => SERVICE_TYPE_NAMES[r.serviceType] ?? r.serviceType },
            { label: "Preacher", key: "preacher" },
            { label: "Online Attendance", key: "onlineAttendance" }
        ],
        rows,
        (row) => `<button class="row-btn delete" onclick="deleteService('${row.id}')">Delete</button>`
    );

    const select = document.getElementById("attendance-service-select");
    select.innerHTML = rows.map((r) =>
        `<option value="${r.id}">${(r.date || "").slice(0, 10)} - ${r.day} (${SERVICE_TYPE_NAMES[r.serviceType] ?? r.serviceType})</option>`
    ).join("") || `<option value="">No services yet - create one first</option>`;
}

document.getElementById("service-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    const body = {
        date: f.date.value,
        day: f.day.value,
        serviceType: parseInt(f.serviceType.value, 10),
        theme: f.theme.value || null,
        scriptureText: f.scriptureText.value || null,
        preacher: f.preacher.value || null,
        onlineAttendance: parseInt(f.onlineAttendance.value, 10) || 0
    };
    const { ok } = await apiRequest("POST", "/api/Service", body);
    if (ok) { f.reset(); loadServices(); } else { alert("Create failed - see log."); }
});

async function deleteService(id) {
    if (!confirm("Delete this service?")) return;
    const { ok } = await apiRequest("DELETE", `/api/Service/${id}`);
    if (ok) loadServices(); else alert("Delete failed - see log.");
}

async function loadAttendance() {
    const { ok, data } = await apiRequest("GET", "/api/Attendance");
    const rows = ok ? unwrap(data.data) : [];

    renderTable("attendance-table",
        [
            { label: "Service Id", render: (r) => r.serviceId.slice(0, 8) + "…" },
            { label: "Men", key: "men" },
            { label: "Women", key: "women" },
            { label: "Children", key: "children" },
            { label: "Total", key: "total" },
            { label: "Approved", render: (r) => r.isApproved ? "Yes" : "No" },
            { label: "Locked", render: (r) => r.isLocked ? "Yes" : "No" }
        ],
        rows,
        (row) => `
            ${!row.isApproved ? `<button class="row-btn approve" onclick="approveAttendance('${row.id}')">Approve</button>` : ""}
            <button class="row-btn delete" onclick="deleteAttendance('${row.id}')">Delete</button>
        `
    );
}

document.getElementById("attendance-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    if (!f.serviceId.value) { alert("Create a Service first (Services tab)."); return; }
    const body = {
        serviceId: f.serviceId.value,
        men: parseInt(f.men.value, 10) || 0,
        women: parseInt(f.women.value, 10) || 0,
        children: parseInt(f.children.value, 10) || 0,
        sundaySchool: parseInt(f.sundaySchool.value, 10) || 0,
        newConverts: parseInt(f.newConverts.value, 10) || 0,
        firstTimers: parseInt(f.firstTimers.value, 10) || 0
    };
    const { ok } = await apiRequest("POST", "/api/Attendance", body);
    if (ok) { f.reset(); loadAttendance(); } else { alert("Create failed - see log."); }
});

async function approveAttendance(id) {
    const { ok } = await apiRequest("POST", `/api/Attendance/${id}/approve`);
    if (ok) loadAttendance(); else alert("Approve failed - see log (only Admin can approve).");
}

async function deleteAttendance(id) {
    if (!confirm("Delete this attendance record?")) return;
    const { ok } = await apiRequest("DELETE", `/api/Attendance/${id}`);
    if (ok) loadAttendance(); else alert("Delete failed - see log.");
}

async function loadFellowshipCenters() {
    const { ok, data } = await apiRequest("GET", "/api/FellowshipCenter");
    const rows = ok ? unwrap(data.data) : [];

    renderTable("fellowship-centers-table",
        [
            { label: "Center Name", key: "centerName" },
            { label: "Zone", key: "zone" },
            { label: "Leader", key: "leaderName" },
            { label: "Location", key: "location" }
        ],
        rows,
        (row) => `<button class="row-btn delete" onclick="deleteFellowshipCenter('${row.id}')">Delete</button>`
    );

    const select = document.getElementById("fellowship-attendance-center-select");
    select.innerHTML = rows.map((r) =>
        `<option value="${r.id}">${r.centerName} (${r.zone})</option>`
    ).join("") || `<option value="">No centers yet - create one first</option>`;
}

document.getElementById("fellowship-center-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    const body = {
        centerName: f.centerName.value,
        zone: f.zone.value,
        leaderName: f.leaderName.value,
        location: f.location.value
    };
    const { ok } = await apiRequest("POST", "/api/FellowshipCenter", body);
    if (ok) { f.reset(); loadFellowshipCenters(); } else { alert("Create failed - see log."); }
});

async function deleteFellowshipCenter(id) {
    if (!confirm("Delete this fellowship center?")) return;
    const { ok } = await apiRequest("DELETE", `/api/FellowshipCenter/${id}`);
    if (ok) loadFellowshipCenters(); else alert("Delete failed - see log.");
}

async function loadFellowshipAttendance() {
    const { ok, data } = await apiRequest("GET", "/api/FellowshipAttendance");
    const rows = ok ? unwrap(data.data) : [];

    renderTable("fellowship-attendance-table",
        [
            { label: "Center Id", render: (r) => r.fellowshipCenterId.slice(0, 8) + "…" },
            { label: "Date", render: (r) => (r.date || "").slice(0, 10) },
            { label: "Men", key: "men" },
            { label: "Women", key: "women" },
            { label: "Children", key: "children" },
            { label: "Total", key: "total" },
            { label: "Approved", render: (r) => r.isApproved ? "Yes" : "No" }
        ],
        rows,
        (row) => `
            ${!row.isApproved ? `<button class="row-btn approve" onclick="approveFellowshipAttendance('${row.id}')">Approve</button>` : ""}
            <button class="row-btn delete" onclick="deleteFellowshipAttendance('${row.id}')">Delete</button>
        `
    );
}

document.getElementById("fellowship-attendance-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    if (!f.fellowshipCenterId.value) { alert("Create a Fellowship Center first."); return; }
    const body = {
        fellowshipCenterId: f.fellowshipCenterId.value,
        date: f.date.value,
        men: parseInt(f.men.value, 10) || 0,
        women: parseInt(f.women.value, 10) || 0,
        children: parseInt(f.children.value, 10) || 0,
        newConverts: parseInt(f.newConverts.value, 10) || 0
    };
    const { ok } = await apiRequest("POST", "/api/FellowshipAttendance", body);
    if (ok) { f.reset(); loadFellowshipAttendance(); } else { alert("Create failed - see log."); }
});

async function approveFellowshipAttendance(id) {
    const { ok } = await apiRequest("POST", `/api/FellowshipAttendance/${id}/approve`);
    if (ok) loadFellowshipAttendance(); else alert("Approve failed - see log (only Admin can approve).");
}

async function deleteFellowshipAttendance(id) {
    if (!confirm("Delete this record?")) return;
    const { ok } = await apiRequest("DELETE", `/api/FellowshipAttendance/${id}`);
    if (ok) loadFellowshipAttendance(); else alert("Delete failed - see log.");
}

async function loadInventory() {
    const { ok, data } = await apiRequest("GET", "/api/InventoryItem");
    const rows = ok ? unwrap(data.data) : [];

    renderTable("inventory-table",
        [
            { label: "Item Name", key: "itemName" },
            { label: "Category", key: "category" },
            { label: "Qty", key: "quantity" },
            { label: "Location", key: "location" },
            { label: "Condition", render: (r) => ITEM_CONDITION_NAMES[r.condition] ?? r.condition },
            { label: "Value", key: "value" },
            { label: "Custodian", key: "custodian" }
        ],
        rows,
        (row) => `<button class="row-btn delete" onclick="deleteInventoryItem('${row.id}')">Delete</button>`
    );
}

document.getElementById("inventory-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    const body = {
        itemName: f.itemName.value,
        category: f.category.value,
        quantity: parseInt(f.quantity.value, 10) || 0,
        location: f.location.value,
        condition: parseInt(f.condition.value, 10),
        purchaseDate: f.purchaseDate.value,
        value: parseFloat(f.value.value) || 0,
        custodian: f.custodian.value,
        lastVerifiedDate: f.lastVerifiedDate.value || null
    };
    const { ok } = await apiRequest("POST", "/api/InventoryItem", body);
    if (ok) { f.reset(); loadInventory(); } else { alert("Create failed - see log."); }
});

async function deleteInventoryItem(id) {
    if (!confirm("Delete this item?")) return;
    const { ok } = await apiRequest("DELETE", `/api/InventoryItem/${id}`);
    if (ok) loadInventory(); else alert("Delete failed - see log.");
}

function refreshAll() {
    loadServices();
    loadAttendance();
    loadFellowshipCenters();
    loadFellowshipAttendance();
    loadInventory();
}

document.querySelectorAll("[data-refresh]").forEach((btn) => {
    btn.addEventListener("click", () => {
        const map = {
            services: loadServices,
            attendance: loadAttendance,
            "fellowship-centers": loadFellowshipCenters,
            "fellowship-attendance": loadFellowshipAttendance,
            inventory: loadInventory
        };
        map[btn.dataset.refresh]();
    });
});

updateAuthStatus();
if (getToken()) refreshAll();
