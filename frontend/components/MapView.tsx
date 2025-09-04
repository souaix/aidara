'use client';

import dynamic from 'next/dynamic';

// 把真正使用 leaflet 的元件動態載入，並關閉 SSR
const MapInner = dynamic(() => import('./MapInner'), { ssr: false });

export default function MapView() {
  return <MapInner />;
}
