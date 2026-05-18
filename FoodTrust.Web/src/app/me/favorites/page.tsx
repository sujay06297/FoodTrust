import { FavoritesClient } from "./favorites-client";

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
