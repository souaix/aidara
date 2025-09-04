'use client';

import { MapContainer, TileLayer, Marker, Popup, useMapEvents } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-cluster';
import L, { LatLngBounds, Map as LeafletMap } from 'leaflet';
import { useEffect, useMemo, useRef, useState } from 'react';
import { searchListings, ListingItem } from '@/lib/api';
import FiltersPanel, { FilterValues } from './FiltersPanel';

const centerMY: [number, number] = [3.139, 101.686]; // Kuala Lumpur

function FetchOnMove({ onBounds }: { onBounds: (b: LatLngBounds) => void }) {
  useMapEvents({
    moveend: (e) => onBounds(e.target.getBounds()),
    zoomend: (e) => onBounds(e.target.getBounds()),
  });
  return null;
}

export default function MapInner() {
  const [items, setItems] = useState<ListingItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<FilterValues>({});
  const mapRef = useRef<LeafletMap | null>(null);
  const timer = useRef<number | undefined>(undefined);

  // 僅在瀏覽器完成第一次掛載後才渲染 MapContainer → 避免「already initialized」
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);

  // 修正 Next 打包圖示路徑
  const defaultIcon = useMemo(() => new L.Icon({
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    iconSize: [25, 41], iconAnchor: [12, 41],
  }), []);

  useEffect(() => {
    return () => {
      if (mapRef.current) {
        mapRef.current.remove();
        mapRef.current = null;
      }
    };
  }, []);

  async function doFetch(b: LatLngBounds) {
    setLoading(true);
    try {
      const sw = b.getSouthWest(); const ne = b.getNorthEast();
      const data = await searchListings({
        minLon: sw.lng, minLat: sw.lat, maxLon: ne.lng, maxLat: ne.lat,
        ...filters, page: 1, pageSize: 200,
      });
      setItems(data.items);
    } finally {
      setLoading(false);
    }
  }

  // 防抖：停止拖動 300ms 後才查詢
  function fetchDebounced(b: LatLngBounds) {
    if (timer.current) window.clearTimeout(timer.current);
    timer.current = window.setTimeout(() => doFetch(b), 300);
  }

  return (
    <div className="w-screen h-screen relative">
      <FiltersPanel value={filters} onChange={(v) => {
        setFilters(v);
        if (mapRef.current) fetchDebounced(mapRef.current.getBounds());
      }} />

      {/* 只有 mounted 後才渲染地圖，並且給一個固定 key，確保容器不被重複初始化 */}
      {mounted && (
        <MapContainer
          center={centerMY}
          zoom={12}
          className="w-full h-full"
          style={{ width: '100%', height: '100%' }}        
          key="leaflet-map"
            ref={(m) => {
            if (m && !mapRef.current) {
                mapRef.current = m;

                // 等地圖真正 ready 再處理尺寸與查詢，避免 _leaflet_pos 未就緒
                m.whenReady(() => {
                m.invalidateSize(true);
                doFetch(m.getBounds());
                });

                // 視窗變動時重新計算尺寸
                const onResize = () => m.invalidateSize(false);
                window.addEventListener('resize', onResize);

                // 清除監聽要放在 useEffect 的 cleanup；這裡只保存參考
                (m as any)._onResize = onResize;
            }
            }}


        >
          <TileLayer
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            attribution="&copy; OpenStreetMap contributors"
          />
          <FetchOnMove onBounds={fetchDebounced} />

          <MarkerClusterGroup chunkedLoading>
            {items.map((x) => (
              <Marker key={x.id} position={[x.lat, x.lon]} icon={defaultIcon}>
                <Popup>
                  <strong>{x.title}</strong><br />
                  單價：{x.priceUnit ?? '-'}　房數：{x.rooms ?? '-'}
                </Popup>
              </Marker>
            ))}
          </MarkerClusterGroup>
        </MapContainer>
      )}

      <div className="absolute bottom-3 left-3 bg-white/90 rounded px-3 py-1 text-sm shadow">
        {loading ? '載入中…' : `目前顯示 ${items.length} 筆`}
      </div>
    </div>
  );
}
