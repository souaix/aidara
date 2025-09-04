// app/page.tsx  （Server Component 保持預設即可）
import MapView from '@/components/MapView'; // MapView 本身是 Client Component（有 'use client'）

export default function Home() {
  return <MapView />;
}
