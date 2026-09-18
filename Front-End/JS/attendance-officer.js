const SERVICE_TYPE_NAMES = ["Sunday", "Wednesday", "Special"];

let servicesCache = [];
let attendanceCache = [];

function getModal(id) {
    return new Modal(document.getElementById(id));
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
        <button class="font-medium text-blue-600 dark:text-blue-500 hover:underline mr-3" onclick="editAttendance('${row.id}')">Edit</button>
        <button class="font-medium text-red-600 dark:text-red-500 hover:underline" onclick="deleteAttendance('${row.id}')">Delete</button>
    `);
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

async function deleteAttendance(id) {
    if (!confirm("Delete this attendance record?")) return;
    const { ok } = await apiRequest("DELETE", `/api/Attendance/${id}`);
    if (ok) { showToast("Attendance deleted."); loadAttendance(); }
    else showToast("Failed to delete attendance.", "error");
}

async function init() {
    initSectionNav();
    await loadServices();
    await loadAttendance();
}

init();
