// next.config.ts
import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  reactStrictMode: false,   // ← 關閉嚴格模式，避免二次掛載
};

export default nextConfig;
