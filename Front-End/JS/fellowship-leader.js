let centersCache = [];
let fellowshipAttendanceCache = [];

function getModal(id) {
    return new Modal(document.getElementById(id));
}

async function loadFellowshipCenters() {
    const { ok, body } = await apiRequest("GET", "/api/FellowshipCenter");
    centersCache = ok ? unwrapList(body) : [];

    renderTableBody("fellowship-centers-tbody", centersCache, [
        { label: "Center Name", render: (r) => r.centerName },
        { label: "Zone", render: (r) => r.zone },
        { label: "Leader", render: (r) => r.leaderName },
        { label: "Location", render: (r) => r.location }
    ], (row) => `
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editCenter('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteCenter('${row.id}')">Delete</button>
    `);

    populateCenterSelect();
}

function populateCenterSelect() {
    const select = document.querySelector(".fellowship-center-select");
    if (!select) return;
    select.innerHTML = centersCache.map((r) =>
        `<option value="${r.id}">${r.centerName} (${r.zone})</option>`
    ).join("") || `<option value="">No centers yet - create one first</option>`;
}

function openCenterModal() {
    const form = document.getElementById("center-form");
    form.reset();
    form.id.value = "";
    document.getElementById("center-modal-title").textContent = "Create Fellowship Center";
    getModal("center-modal").show();
}

function editCenter(id) {
    const row = centersCache.find((r) => r.id === id);
    if (!row) return;
    const form = document.getElementById("center-form");
    form.id.value = row.id;
    form.centerName.value = row.centerName || "";
    form.zone.value = row.zone || "";
    form.leaderName.value = row.leaderName || "";
    form.location.value = row.location || "";
    document.getElementById("center-modal-title").textContent = "Edit Fellowship Center";
    getModal("center-modal").show();
}

document.getElementById("center-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    const body = {
        centerName: f.centerName.value,
        zone: f.zone.value,
        leaderName: f.leaderName.value,
        location: f.location.value
    };
    const id = f.id.value;
    const { ok } = id
        ? await apiRequest("PUT", `/api/FellowshipCenter/${id}`, { ...body, id })
        : await apiRequest("POST", "/api/FellowshipCenter", body);

    if (ok) {
        showToast(id ? "Fellowship center updated." : "Fellowship center created.");
        getModal("center-modal").hide();
        loadFellowshipCenters();
    } else {
        showToast("Failed to save fellowship center.", "error");
    }
});

async function deleteCenter(id) {
    if (!confirm("Delete this fellowship center?")) return;
    const { ok } = await apiRequest("DELETE", `/api/FellowshipCenter/${id}`);
    if (ok) { showToast("Fellowship center deleted."); loadFellowshipCenters(); }
    else showToast("Failed to delete fellowship center.", "error");
}

async function loadFellowshipAttendance() {
    const { ok, body } = await apiRequest("GET", "/api/FellowshipAttendance");
    fellowshipAttendanceCache = ok ? unwrapList(body) : [];

    renderTableBody("fellowship-attendance-tbody", fellowshipAttendanceCache, [
        { label: "Center", render: (r) => centerLabel(r.fellowshipCenterId) },
        { label: "Date", render: (r) => (r.date || "").slice(0, 10) },
        { label: "Men", render: (r) => r.men },
        { label: "Women", render: (r) => r.women },
        { label: "Children", render: (r) => r.children },
        { label: "New Converts", render: (r) => r.newConverts },
        { label: "Total", render: (r) => `<span class="font-semibold">${r.total}</span>` },
        { label: "Status", render: (r) => statusBadge(r.isApproved) + lockBadge(r.isLocked) }
    ], (row) => `
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editFellowshipAttendance('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteFellowshipAttendance('${row.id}')">Delete</button>
    `);
}

function centerLabel(id) {
    const c = centersCache.find((x) => x.id === id);
    return c ? c.centerName : id.slice(0, 8) + "…";
}

function openFellowshipAttendanceModal() {
    if (!centersCache.length) { showToast("Create a Fellowship Center first.", "error"); return; }
    const form = document.getElementById("fellowship-attendance-form");
    form.reset();
    form.id.value = "";
    populateCenterSelect();
    document.getElementById("fellowship-attendance-modal-title").textContent = "Create Fellowship Attendance";
    getModal("fellowship-attendance-modal").show();
}

function editFellowshipAttendance(id) {
    const row = fellowshipAttendanceCache.find((r) => r.id === id);
    if (!row) return;
    const form = document.getElementById("fellowship-attendance-form");
    populateCenterSelect();
    form.id.value = row.id;
    form.fellowshipCenterId.value = row.fellowshipCenterId;
    form.date.value = (row.date || "").slice(0, 10);
    form.men.value = row.men;
    form.women.value = row.women;
    form.children.value = row.children;
    form.newConverts.value = row.newConverts;
    document.getElementById("fellowship-attendance-modal-title").textContent = "Edit Fellowship Attendance";
    getModal("fellowship-attendance-modal").show();
}

document.getElementById("fellowship-attendance-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    const body = {
        fellowshipCenterId: f.fellowshipCenterId.value,
        date: f.date.value,
        men: parseInt(f.men.value, 10) || 0,
        women: parseInt(f.women.value, 10) || 0,
        children: parseInt(f.children.value, 10) || 0,
        newConverts: parseInt(f.newConverts.value, 10) || 0
    };
    const id = f.id.value;
    const { ok } = id
        ? await apiRequest("PUT", `/api/FellowshipAttendance/${id}`, { ...body, id })
        : await apiRequest("POST", "/api/FellowshipAttendance", body);

    if (ok) {
        showToast(id ? "Fellowship attendance updated." : "Fellowship attendance created.");
        getModal("fellowship-attendance-modal").hide();
        loadFellowshipAttendance();
    } else {
        showToast("Failed to save fellowship attendance.", "error");
    }
});

async function deleteFellowshipAttendance(id) {
    if (!confirm("Delete this fellowship attendance record?")) return;
    const { ok } = await apiRequest("DELETE", `/api/FellowshipAttendance/${id}`);
    if (ok) { showToast("Fellowship attendance deleted."); loadFellowshipAttendance(); }
    else showToast("Failed to delete fellowship attendance.", "error");
}

async function init() {
    initSectionNav();
    await loadFellowshipCenters();
    await loadFellowshipAttendance();
}

init();
