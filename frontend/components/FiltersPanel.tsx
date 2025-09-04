'use client';
import { useState } from 'react';

export type FilterValues = {
  minUnit?: number; maxUnit?: number;
  minRooms?: number; maxRooms?: number;
};

export default function FiltersPanel({
  value, onChange,
}: { value: FilterValues; onChange: (v: FilterValues) => void }) {

  const [v, setV] = useState<FilterValues>(value);

  function set(key: keyof FilterValues, val: string) {
    const num = val === '' ? undefined : Number(val);
    const next = { ...v, [key]: Number.isNaN(num!) ? undefined : num };
    setV(next);
    onChange(next);
  }

  return (
    <div className="absolute top-3 left-3 z-[1000] bg-white/95 rounded-xl shadow p-3 flex gap-3 text-sm">
      <label className="flex items-center gap-1">
        單價：
        <input className="w-20 border rounded px-2 py-1" placeholder="min"
               onChange={(e)=>set('minUnit', e.target.value)} />
        <span>–</span>
        <input className="w-20 border rounded px-2 py-1" placeholder="max"
               onChange={(e)=>set('maxUnit', e.target.value)} />
      </label>

      <label className="flex items-center gap-1">
        房數：
        <input className="w-16 border rounded px-2 py-1" placeholder="min"
               onChange={(e)=>set('minRooms', e.target.value)} />
        <span>–</span>
        <input className="w-16 border rounded px-2 py-1" placeholder="max"
               onChange={(e)=>set('maxRooms', e.target.value)} />
      </label>
    </div>
  );
}
