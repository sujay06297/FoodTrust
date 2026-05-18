import Link from "next/link";
import type { Metadata } from "next";
import { RestaurantCard } from "@/components/restaurants/restaurant-card";
import { RestaurantSearchForm } from "@/components/restaurants/restaurant-search-form";
import { getRestaurantRankings } from "@/lib/api/restaurants";
import type { RestaurantRankingItem } from "@/lib/api/types";

export const metadata: Metadata = {
  title: "可信賴的美食排行",
  description: "依平台分數、有效評論與收藏訊號探索可信賴的餐廳排行。",
};

export default async function Home() {
  let rankings: RestaurantRankingItem[] = [];

  try {
    rankings = await getRestaurantRankings(8);
  } catch {
    rankings = [];
  }

  return (
    <main className="mx-auto grid max-w-6xl gap-6 px-4 py-6">
      <section className="grid gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-zinc-950">找可信賴的餐廳排行</h1>
          <p className="mt-2 text-sm text-zinc-600">
            依平台分數、有效評論與收藏訊號篩選餐廳。
          </p>
        </div>
        <RestaurantSearchForm defaultValues={{ sortBy: "ranking" }} />
      </section>

      <section className="border border-zinc-200 bg-white">
        <div className="flex items-center justify-between border-b border-zinc-200 px-4 py-3">
          <h2 className="font-semibold text-zinc-950">熱門排行</h2>
          <Link className="text-sm text-zinc-600 hover:text-zinc-950" href="/restaurants?sortBy=ranking">
            查看更多
          </Link>
        </div>
        {rankings.length > 0 ? (
          <div>
            {rankings.map((restaurant) => (
              <RestaurantCard key={restaurant.id} restaurant={restaurant} />
            ))}
          </div>
        ) : (
          <p className="p-4 text-sm text-zinc-600">目前無法載入排行，請確認 API 是否啟動。</p>
        )}
      </section>
    </main>
  );
}
