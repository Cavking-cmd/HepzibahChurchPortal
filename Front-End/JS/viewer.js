const SERVICE_TYPE_NAMES = ["Sunday", "Wednesday", "Special"];
const ITEM_CONDITION_NAMES = ["Good", "Needs Repair", "Replace"];

let servicesCache = [];
let centersCache = [];

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
    ]);
}

async function loadAttendance() {
    const { ok, body } = await apiRequest("GET", "/api/Attendance");
    const rows = ok ? unwrapList(body) : [];

    renderTableBody("attendance-tbody", rows, [
        { label: "Service", render: (r) => serviceLabel(r.serviceId) },
        { label: "Men", render: (r) => r.men },
        { label: "Women", render: (r) => r.women },
        { label: "Children", render: (r) => r.children },
        { label: "Sunday School", render: (r) => r.sundaySchool },
        { label: "New Converts", render: (r) => r.newConverts },
        { label: "First Timers", render: (r) => r.firstTimers },
        { label: "Total", render: (r) => `<span class="font-semibold">${r.total}</span>` },
        { label: "Status", render: (r) => statusBadge(r.isApproved) + lockBadge(r.isLocked) }
    ]);
}

function serviceLabel(serviceId) {
    const s = servicesCache.find((x) => x.id === serviceId);
    return s ? `${(s.date || "").slice(0, 10)} (${s.day})` : serviceId.slice(0, 8) + "…";
}

async function loadFellowshipCenters() {
    const { ok, body } = await apiRequest("GET", "/api/FellowshipCenter");
    centersCache = ok ? unwrapList(body) : [];

    renderTableBody("fellowship-centers-tbody", centersCache, [
        { label: "Center Name", render: (r) => r.centerName },
        { label: "Zone", render: (r) => r.zone },
        { label: "Leader", render: (r) => r.leaderName },
        { label: "Location", render: (r) => r.location }
    ]);
}

function centerLabel(id) {
    const c = centersCache.find((x) => x.id === id);
    return c ? c.centerName : id.slice(0, 8) + "…";
}

async function loadFellowshipAttendance() {
    const { ok, body } = await apiRequest("GET", "/api/FellowshipAttendance");
    const rows = ok ? unwrapList(body) : [];

    renderTableBody("fellowship-attendance-tbody", rows, [
        { label: "Center", render: (r) => centerLabel(r.fellowshipCenterId) },
        { label: "Date", render: (r) => (r.date || "").slice(0, 10) },
        { label: "Men", render: (r) => r.men },
        { label: "Women", render: (r) => r.women },
        { label: "Children", render: (r) => r.children },
        { label: "New Converts", render: (r) => r.newConverts },
        { label: "Total", render: (r) => `<span class="font-semibold">${r.total}</span>` },
        { label: "Status", render: (r) => statusBadge(r.isApproved) + lockBadge(r.isLocked) }
    ]);
}

async function loadInventory() {
    const { ok, body } = await apiRequest("GET", "/api/InventoryItem");
    const rows = ok ? unwrapList(body) : [];

    renderTableBody("inventory-tbody", rows, [
        { label: "Item Name", render: (r) => r.itemName },
        { label: "Category", render: (r) => r.category },
        { label: "Qty", render: (r) => r.quantity },
        { label: "Location", render: (r) => r.location },
        { label: "Condition", render: (r) => ITEM_CONDITION_NAMES[r.condition] ?? r.condition },
        { label: "Value", render: (r) => Number(r.value ?? 0).toLocaleString(undefined, { style: "currency", currency: "USD" }) },
        { label: "Custodian", render: (r) => r.custodian }
    ]);
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
