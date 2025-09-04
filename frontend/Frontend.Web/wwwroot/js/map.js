// ===== 全域常數 =====
const API_BASE = document.querySelector('meta[name=api-base]')?.content || "";
const apiUrl = (path) => (API_BASE ? `${API_BASE}${path}` : path);

// ===== 全域變數 =====
let map;
let markers;
let zonesLayer;
let debounceTimer;

// 共用：取 JSON（處理非 200）
function fetchJson(url) {
    return fetch(url).then(resp => {
        if (!resp.ok) throw new Error(`${url} -> ${resp.status}`);
        return resp.json();
    });
}

// ===== DOM Ready =====
document.addEventListener("DOMContentLoaded", function () {
    // 1) 地圖
    map = L.map('map').setView([1.4927, 103.7414], 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    // 2) Cluster / Zones
    markers = L.markerClusterGroup({
        showCoverageOnHover: false,
        zoomToBoundsOnClick: true,
        spiderfyOnMaxZoom: true
    });
    map.addLayer(markers);

    zonesLayer = L.layerGroup();
    map.addLayer(zonesLayer);

    // 3) 單一 debounced 事件：先載區筆數，再載清單
    const scheduleRefresh = () => {
        if (debounceTimer) clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            Promise.all([loadZoneCounts(), loadListings()]).catch(console.error);
        }, 300);
    };

    map.on("moveend zoomend", scheduleRefresh);
    const btn = document.getElementById("btnSearch");
    if (btn) btn.addEventListener("click", scheduleRefresh);

    scheduleRefresh(); // 初次載入
});

// ===== 載入各區筆數 =====
function loadZoneCounts() {
    zonesLayer.clearLayers();
    const b = map.getBounds();
    const qs = new URLSearchParams({
        minLon: b.getWest(), minLat: b.getSouth(),
        maxLon: b.getEast(), maxLat: b.getNorth()
    });

    return fetchJson(apiUrl(`/api/v1/zones/with-count?${qs.toString()}`))
        .then(data => {
            data.forEach(z => {
                const html = `<div style="background:#006699;color:#fff;border-radius:16px;
          padding:4px 8px;font-weight:700;box-shadow:0 1px 3px rgba(0,0,0,.3);white-space:nowrap;">
          ${z.name}: ${z.count}</div>`;
                const icon = L.divIcon({ className: "zone-count", html, iconAnchor: [0, 0] });
                const m = L.marker([z.lat, z.lon], { icon });
                m.on("click", () => map.setView([z.lat, z.lon], Math.min(map.getZoom() + 2, 16)));
                zonesLayer.addLayer(m);
            });
        })
        .catch(e => {
            console.error("載入區筆數失敗", e);
        });
}

// ===== 載入 listings =====
function loadListings() {
    markers.clearLayers();

    const b = map.getBounds();
    const qs = new URLSearchParams({
        minLon: b.getWest(), minLat: b.getSouth(),
        maxLon: b.getEast(), maxLat: b.getNorth(),
        minUnit: document.getElementById("minUnit")?.value || "",
        maxUnit: document.getElementById("maxUnit")?.value || "",
        minRooms: document.getElementById("minRooms")?.value || "",
        maxRooms: document.getElementById("maxRooms")?.value || "",
        page: 1, pageSize: 200
    });

    return fetchJson(apiUrl(`/api/v1/listings/search?${qs.toString()}`))
        .then(data => {
            const badge = document.getElementById("totalBadge");
            if (badge) badge.textContent = `共 ${data.total} 筆`;

            data.items.forEach(item => {
                const m = L.marker([item.lat, item.lon]);
                m.bindPopup(`<b>${item.title}</b><br/>單價: ${item.priceUnit ?? "-"}<br/>房數: ${item.rooms ?? "-"}`);
                markers.addLayer(m);
            });
        })
        .catch(e => {
            console.error("載入清單失敗", e);
            alert("載入清單失敗，請開 F12 檢查 /api 回應與網路錯誤。");
        });
}
