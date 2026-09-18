const DEFAULT_API_BASE = "http://localhost:5080";

function getApiBase() {
    const stored = localStorage.getItem("apiBase");
    return (stored || DEFAULT_API_BASE).replace(/\/$/, "");
}

function setApiBase(value) {
    localStorage.setItem("apiBase", value);
}

function getToken() {
    return localStorage.getItem("token") || "";
}

function getUser() {
    const raw = localStorage.getItem("user");
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
}

function getRoles() {
    const raw = localStorage.getItem("roles");
    if (!raw) return [];
    try { return JSON.parse(raw); } catch { return []; }
}

function unwrap(value) {
    if (value && typeof value === "object" && Array.isArray(value.$values)) {
        return value.$values;
    }
    if (Array.isArray(value)) return value;
    return value ?? [];
}

async function apiRequest(method, path, body) {
    const headers = { "Content-Type": "application/json" };
    const token = getToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;

    let response;
    try {
        response = await fetch(`${getApiBase()}${path}`, {
            method,
            headers,
            body: body !== undefined ? JSON.stringify(body) : undefined
        });
    } catch (err) {
        return { ok: false, status: 0, body: { message: "Network error - is the API running at " + getApiBase() + "?" } };
    }

    let parsed = null;
    const text = await response.text();
    if (text) {
        try { parsed = JSON.parse(text); } catch { parsed = { message: text }; }
    }

    return { ok: response.ok, status: response.status, body: parsed };
}

function unwrapList(body) {
    if (!body) return [];
    return unwrap(body.data);
}

function ensureToastContainer() {
    let container = document.getElementById("toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container";
        container.className = "fixed top-4 right-4 z-50 flex flex-col gap-2";
        document.body.appendChild(container);
    }
    return container;
}

function renderTableBody(tbodyId, rows, columns, actions) {
    const tbody = document.getElementById(tbodyId);
    if (!tbody) return;
    const colCount = columns.length + (actions ? 1 : 0);

    if (!rows.length) {
        tbody.innerHTML = `<tr><td colspan="${colCount}" class="px-4 py-6 text-center text-gray-400">No records yet.</td></tr>`;
        return;
    }

    tbody.innerHTML = rows.map((row) => {
        const cells = columns.map((c) =>
            `<td class="px-4 py-3">${c.render(row)}</td>`
        ).join("");
        const actionCell = actions
            ? `<td class="px-4 py-3 text-right whitespace-nowrap">${actions(row)}</td>`
            : "";
        return `<tr class="bg-white border-b dark:bg-gray-800 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50">${cells}${actionCell}</tr>`;
    }).join("");
}

function statusBadge(isApproved) {
    return isApproved
        ? '<span class="bg-green-100 text-green-800 text-xs font-medium me-2 px-2.5 py-0.5 rounded-full dark:bg-green-900 dark:text-green-300">Approved</span>'
        : '<span class="bg-yellow-100 text-yellow-800 text-xs font-medium me-2 px-2.5 py-0.5 rounded-full dark:bg-yellow-900 dark:text-yellow-300">Pending</span>';
}

function lockBadge(isLocked) {
    return isLocked
        ? '<span class="bg-gray-100 text-gray-800 text-xs font-medium me-2 px-2.5 py-0.5 rounded-full dark:bg-gray-700 dark:text-gray-300">Locked</span>'
        : "";
}

function initSectionNav() {
    const links = document.querySelectorAll("[data-section]");
    links.forEach((link) => {
        link.addEventListener("click", (e) => {
            e.preventDefault();
            const target = link.dataset.section;
            links.forEach((l) => l.classList.remove("active"));
            link.classList.add("active");
            document.querySelectorAll(".section-panel").forEach((p) => p.classList.remove("active"));
            const panel = document.getElementById("section-" + target);
            if (panel) panel.classList.add("active");
        });
    });
}

function showToast(message, type) {
    const container = ensureToastContainer();
    const isError = type === "error";

    const toast = document.createElement("div");
    toast.className = "flex items-center w-full max-w-xs p-4 text-gray-500 bg-white rounded-lg shadow-sm dark:text-gray-400 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 animate-fade-in";
    toast.setAttribute("role", "alert");

    const iconWrapClass = isError
        ? "inline-flex items-center justify-center shrink-0 w-8 h-8 text-red-500 bg-red-100 rounded-lg dark:bg-red-800 dark:text-red-200"
        : "inline-flex items-center justify-center shrink-0 w-8 h-8 text-green-500 bg-green-100 rounded-lg dark:bg-green-800 dark:text-green-200";

    const iconSvg = isError
        ? '<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/></svg>'
        : '<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/></svg>';

    toast.innerHTML = `
        <div class="${iconWrapClass}">${iconSvg}</div>
        <div class="ms-3 text-sm font-normal">${message}</div>
        <button type="button" class="ms-auto -mx-1.5 -my-1.5 bg-white text-gray-400 hover:text-gray-900 rounded-lg focus:ring-2 focus:ring-gray-300 p-1.5 hover:bg-gray-100 inline-flex items-center justify-center h-8 w-8 dark:text-gray-500 dark:hover:text-white dark:bg-gray-800 dark:hover:bg-gray-700">
            <svg class="w-3 h-3" fill="none" viewBox="0 0 14 14"><path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M1 1l6 6m0 0l6 6M7 7l6-6M7 7l-6 6"/></svg>
        </button>
    `;

    toast.querySelector("button").addEventListener("click", () => toast.remove());
    container.appendChild(toast);
    setTimeout(() => toast.remove(), 5000);
}
