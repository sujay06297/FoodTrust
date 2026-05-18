import { RestaurantCard } from "@/components/restaurants/restaurant-card";
import { RestaurantSearchForm } from "@/components/restaurants/restaurant-search-form";
import { searchRestaurants, type RestaurantSearchParams } from "@/lib/api/restaurants";

type PageProps = {
  searchParams: Promise<RestaurantSearchParams>;
};

export default async function RestaurantsPage({ searchParams }: PageProps) {
  const params = await searchParams;
  let result = null;

  try {
    result = await searchRestaurants(params);
  } catch {
    result = null;
  }

  return (
    <main className="mx-auto grid max-w-6xl gap-4 px-4 py-6">
      <div>
        <h1 className="text-xl font-semibold text-zinc-950">餐廳搜尋</h1>
        <p className="mt-1 text-sm text-zinc-600">用地區、料理與排序快速縮小選擇。</p>
      </div>
      <section className="border border-zinc-200 bg-white">
        <RestaurantSearchForm defaultValues={params} />
        {result ? (
          <>
            <div className="border-b border-zinc-200 px-4 py-3 text-sm text-zinc-600">
              共 {result.totalCount} 間餐廳
            </div>
            {result.items.map((restaurant) => (
              <RestaurantCard key={restaurant.id} restaurant={restaurant} />
            ))}
            {result.items.length === 0 ? (
              <p className="p-4 text-sm text-zinc-600">沒有符合條件的餐廳。</p>
            ) : null}
          </>
        ) : (
          <p className="p-4 text-sm text-zinc-600">目前無法載入餐廳列表，請確認 API 是否啟動。</p>
        )}
      </section>
    </main>
  );
}
