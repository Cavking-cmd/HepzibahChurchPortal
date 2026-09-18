const SERVICE_TYPE_NAMES = ["Sunday", "Wednesday", "Special"];
const ITEM_CONDITION_NAMES = ["Good", "Needs Repair", "Replace"];

let servicesCache = [];
let centersCache = [];

function getModal(id) {
    const el = document.getElementById(id);
    return new Modal(el);
}

async function loadServices() {
    const { ok, body } = await apiRequest("GET", "/api/Service");
    servicesCache = ok ? unwrapList(body) : [];

    renderTableBody("services-tbody", servicesCache, [
        { label: "Date", render: (r) => (r.date || "").slice(0, 10) },
        { label: "Day", render: (r) => r.day || "" },
        { label: "Type", render: (r) => SERVICE_TYPE_NAMES[r.serviceType] ?? r.serviceType },
        { label: "Preacher", render: (r) => r.preacher || "-" },
        { label: "Theme", render: (r) => r.theme || "-" },
        { label: "Online Attendance", render: (r) => r.onlineAttendance ?? 0 }
    ], (row) => `
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editService('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteService('${row.id}')">Delete</button>
    `);

    populateServiceSelect();
    document.getElementById("stat-services").textContent = servicesCache.length;
}

function populateServiceSelect() {
    const select = document.querySelector(".attendance-service-select");
    if (!select) return;
    select.innerHTML = servicesCache.map((r) =>
        `<option value="${r.id}">${(r.date || "").slice(0, 10)} - ${r.day} (${SERVICE_TYPE_NAMES[r.serviceType] ?? r.serviceType})</option>`
    ).join("") || `<option value="">No services yet - create one first</option>`;
}

function openServiceModal() {
    const form = document.getElementById("service-form");
    form.reset();
    form.id.value = "";
    document.getElementById("service-modal-title").textContent = "Create Service";
    getModal("service-modal").show();
}

function editService(id) {
    const row = servicesCache.find((r) => r.id === id);
    if (!row) return;
    const form = document.getElementById("service-form");
    form.id.value = row.id;
    form.date.value = (row.date || "").slice(0, 10);
    form.day.value = row.day || "";
    form.serviceType.value = row.serviceType;
    form.onlineAttendance.value = row.onlineAttendance ?? 0;
    form.preacher.value = row.preacher || "";
    form.theme.value = row.theme || "";
    form.scriptureText.value = row.scriptureText || "";
    document.getElementById("service-modal-title").textContent = "Edit Service";
    getModal("service-modal").show();
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
    const id = f.id.value;
    const { ok } = id
        ? await apiRequest("PUT", `/api/Service/${id}`, { ...body, id })
        : await apiRequest("POST", "/api/Service", body);

    if (ok) {
        showToast(id ? "Service updated." : "Service created.");
        getModal("service-modal").hide();
        loadServices();
    } else {
        showToast("Failed to save service.", "error");
    }
});

async function deleteService(id) {
    if (!confirm("Delete this service? This cannot be undone.")) return;
    const { ok } = await apiRequest("DELETE", `/api/Service/${id}`);
    if (ok) { showToast("Service deleted."); loadServices(); }
    else showToast("Failed to delete service.", "error");
}

let attendanceCache = [];

async function loadAttendance() {
    const { ok, body } = await apiRequest("GET", "/api/Attendance");
    attendanceCache = ok ? unwrapList(body) : [];

    renderTableBody("attendance-tbody", attendanceCache, [
        { label: "Service", render: (r) => serviceLabel(r.serviceId) },
        { label: "Men", render: (r) => r.men },
        { label: "Women", render: (r) => r.women },
        { label: "Children", render: (r) => r.children },
        { label: "Sunday School", render: (r) => r.sundaySchool },
        { label: "New Converts", render: (r) => r.newConverts },
        { label: "First Timers", render: (r) => r.firstTimers },
        { label: "Total", render: (r) => `<span class="font-semibold">${r.total}</span>` },
        { label: "Status", render: (r) => statusBadge(r.isApproved) + lockBadge(r.isLocked) }
    ], (row) => `
        ${!row.isApproved ? `<button class="font-medium text-green-600 dark:text-green-500 hover:underline mr-3" onclick="approveAttendance('${row.id}')">Approve</button>` : ""}
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editAttendance('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteAttendance('${row.id}')">Delete</button>
    `);

    const pending = attendanceCache.filter((r) => !r.isApproved).length
        + fellowshipAttendanceCache.filter((r) => !r.isApproved).length;
    document.getElementById("stat-pending").textContent = pending;
}

function serviceLabel(serviceId) {
    const s = servicesCache.find((x) => x.id === serviceId);
    return s ? `${(s.date || "").slice(0, 10)} (${s.day})` : serviceId.slice(0, 8) + "…";
}

function openAttendanceModal() {
    if (!servicesCache.length) { showToast("Create a Service first.", "error"); return; }
    const form = document.getElementById("attendance-form");
    form.reset();
    form.id.value = "";
    populateServiceSelect();
    document.getElementById("attendance-modal-title").textContent = "Create Attendance";
    getModal("attendance-modal").show();
}

function editAttendance(id) {
    const row = attendanceCache.find((r) => r.id === id);
    if (!row) return;
    const form = document.getElementById("attendance-form");
    populateServiceSelect();
    form.id.value = row.id;
    form.serviceId.value = row.serviceId;
    form.men.value = row.men;
    form.women.value = row.women;
    form.children.value = row.children;
    form.sundaySchool.value = row.sundaySchool;
    form.newConverts.value = row.newConverts;
    form.firstTimers.value = row.firstTimers;
    document.getElementById("attendance-modal-title").textContent = "Edit Attendance";
    getModal("attendance-modal").show();
}

document.getElementById("attendance-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const f = e.target;
    const body = {
        serviceId: f.serviceId.value,
        men: parseInt(f.men.value, 10) || 0,
        women: parseInt(f.women.value, 10) || 0,
        children: parseInt(f.children.value, 10) || 0,
        sundaySchool: parseInt(f.sundaySchool.value, 10) || 0,
        newConverts: parseInt(f.newConverts.value, 10) || 0,
        firstTimers: parseInt(f.firstTimers.value, 10) || 0
    };
    const id = f.id.value;
    const { ok } = id
        ? await apiRequest("PUT", `/api/Attendance/${id}`, { ...body, id })
        : await apiRequest("POST", "/api/Attendance", body);

    if (ok) {
        showToast(id ? "Attendance updated." : "Attendance created.");
        getModal("attendance-modal").hide();
        loadAttendance();
    } else {
        showToast("Failed to save attendance.", "error");
    }
});

async function approveAttendance(id) {
    const { ok } = await apiRequest("POST", `/api/Attendance/${id}/approve`);
    if (ok) { showToast("Attendance approved."); loadAttendance(); }
    else showToast("Failed to approve attendance.", "error");
}

async function deleteAttendance(id) {
    if (!confirm("Delete this attendance record?")) return;
    const { ok } = await apiRequest("DELETE", `/api/Attendance/${id}`);
    if (ok) { showToast("Attendance deleted."); loadAttendance(); }
    else showToast("Failed to delete attendance.", "error");
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
    document.getElementById("stat-centers").textContent = centersCache.length;
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

let fellowshipAttendanceCache = [];

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
        ${!row.isApproved ? `<button class="font-medium text-green-600 dark:text-green-500 hover:underline mr-3" onclick="approveFellowshipAttendance('${row.id}')">Approve</button>` : ""}
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editFellowshipAttendance('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteFellowshipAttendance('${row.id}')">Delete</button>
    `);

    const pending = attendanceCache.filter((r) => !r.isApproved).length
        + fellowshipAttendanceCache.filter((r) => !r.isApproved).length;
    document.getElementById("stat-pending").textContent = pending;
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

async function approveFellowshipAttendance(id) {
    const { ok } = await apiRequest("POST", `/api/FellowshipAttendance/${id}/approve`);
    if (ok) { showToast("Fellowship attendance approved."); loadFellowshipAttendance(); }
    else showToast("Failed to approve fellowship attendance.", "error");
}

async function deleteFellowshipAttendance(id) {
    if (!confirm("Delete this fellowship attendance record?")) return;
    const { ok } = await apiRequest("DELETE", `/api/FellowshipAttendance/${id}`);
    if (ok) { showToast("Fellowship attendance deleted."); loadFellowshipAttendance(); }
    else showToast("Failed to delete fellowship attendance.", "error");
}

let inventoryCache = [];

async function loadInventory() {
    const { ok, body } = await apiRequest("GET", "/api/InventoryItem");
    inventoryCache = ok ? unwrapList(body) : [];

    renderTableBody("inventory-tbody", inventoryCache, [
        { label: "Item Name", render: (r) => r.itemName },
        { label: "Category", render: (r) => r.category },
        { label: "Qty", render: (r) => r.quantity },
        { label: "Location", render: (r) => r.location },
        { label: "Condition", render: (r) => ITEM_CONDITION_NAMES[r.condition] ?? r.condition },
        { label: "Value", render: (r) => Number(r.value ?? 0).toLocaleString(undefined, { style: "currency", currency: "USD" }) },
        { label: "Custodian", render: (r) => r.custodian }
    ], (row) => `
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editInventoryItem('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteInventoryItem('${row.id}')">Delete</button>
    `);

    const totalValue = inventoryCache.reduce((sum, r) => sum + (Number(r.value) || 0), 0);
    document.getElementById("stat-inventory-value").textContent =
        totalValue.toLocaleString(undefined, { style: "currency", currency: "USD" });
}

function openInventoryModal() {
    const form = document.getElementById("inventory-form");
    form.reset();
    form.id.value = "";
    document.getElementById("inventory-modal-title").textContent = "Create Inventory Item";
    getModal("inventory-modal").show();
}

function editInventoryItem(id) {
    const row = inventoryCache.find((r) => r.id === id);
    if (!row) return;
    const form = document.getElementById("inventory-form");
    form.id.value = row.id;
    form.itemName.value = row.itemName || "";
    form.category.value = row.category || "";
    form.quantity.value = row.quantity ?? 0;
    form.location.value = row.location || "";
    form.condition.value = row.condition;
    form.value.value = row.value ?? 0;
    form.custodian.value = row.custodian || "";
    form.purchaseDate.value = (row.purchaseDate || "").slice(0, 10);
    form.lastVerifiedDate.value = (row.lastVerifiedDate || "").slice(0, 10);
    document.getElementById("inventory-modal-title").textContent = "Edit Inventory Item";
    getModal("inventory-modal").show();
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
    const id = f.id.value;
    const { ok } = id
        ? await apiRequest("PUT", `/api/InventoryItem/${id}`, { ...body, id })
        : await apiRequest("POST", "/api/InventoryItem", body);

    if (ok) {
        showToast(id ? "Inventory item updated." : "Inventory item created.");
        getModal("inventory-modal").hide();
        loadInventory();
    } else {
        showToast("Failed to save inventory item.", "error");
    }
});

async function deleteInventoryItem(id) {
    if (!confirm("Delete this inventory item?")) return;
    const { ok } = await apiRequest("DELETE", `/api/InventoryItem/${id}`);
    if (ok) { showToast("Inventory item deleted."); loadInventory(); }
    else showToast("Failed to delete inventory item.", "error");
}

async function init() {
    initSectionNav();
    await loadServices();
    await loadFellowshipCenters();
    await loadAttendance();
    await loadFellowshipAttendance();
    await loadInventory();
}

init();
