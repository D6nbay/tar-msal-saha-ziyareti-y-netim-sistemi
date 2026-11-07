import type { NextConfig } from "next";

const apiUrl = process.env.API_URL ?? "http://localhost:5139";
// Backend adresi

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${apiUrl}/api/:path*`,
        // Tarayıcıdan gelen api istekleri backend'e yönlendirilir
      },
    ];
  },
};

export default nextConfig;