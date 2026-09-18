const ITEM_CONDITION_NAMES = ["Good", "Needs Repair", "Replace"];

let inventoryCache = [];

function getModal(id) {
    return new Modal(document.getElementById(id));
}

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
    await loadInventory();
}

init();
