import type { Metadata } from "next";
import { FavoritesClient } from "./favorites-client";

export const metadata: Metadata = {
  title: "我的收藏",
  description: "查看 FoodTrust 食信會員已收藏的餐廳清單。",
  robots: {
    index: false,
    follow: false,
  },
};

export default function FavoritesPage() {
  return (
    <main className="mx-auto grid max-w-6xl gap-4 px-4 py-6">
      <div>
        <h1 className="text-xl font-semibold text-zinc-950">我的收藏</h1>
        <p className="mt-1 text-sm text-zinc-600">查看已收藏的餐廳。</p>
      </div>
      <FavoritesClient />
    </main>
  );
}
