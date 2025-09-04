export type ListingItem = {
  id: number;
  title: string;
  priceUnit?: number | null;
  rooms?: number | null;
  lon: number;
  lat: number;
};

export type SearchResp = {
  total: number;
  items: ListingItem[];
};

const API = process.env.NEXT_PUBLIC_API_BASE ?? 'http://localhost:7030';

export async function searchListings(params: {
  minLon: number; minLat: number; maxLon: number; maxLat: number;
  minUnit?: number; maxUnit?: number;
  minRooms?: number; maxRooms?: number;
  page?: number; pageSize?: number;
}) {
  const qs = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null) qs.set(k, String(v));
  });
  const r = await fetch(`${API}/api/v1/listings/search?${qs.toString()}`, { cache: 'no-store' });
  if (!r.ok) throw new Error(`API ${r.status}`);
  return (await r.json()) as SearchResp;
}
