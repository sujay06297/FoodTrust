import Link from "next/link";
import type { RestaurantListItem, RestaurantRankingItem } from "@/lib/api/types";

type CardRestaurant = RestaurantListItem | (RestaurantRankingItem & {
  branchName?: string | null;
  city?: string | null;
  district?: string | null;
  priceMin?: number | null;
  priceMax?: number | null;
  cuisineType?: string | null;
});

export function RestaurantCard({ restaurant }: { restaurant: CardRestaurant }) {
  const location = [restaurant.city, restaurant.district].filter(Boolean).join(" ");
  const price =
    restaurant.priceMin || restaurant.priceMax
      ? `$${restaurant.priceMin ?? "?"} - $${restaurant.priceMax ?? "?"}`
      : "價位未提供";

  return (
    <Link
      href={`/restaurants/${restaurant.id}`}
      className="grid gap-3 border-b border-zinc-200 bg-white px-4 py-4 transition hover:bg-zinc-50 sm:grid-cols-[1fr_auto]"
    >
      <div className="min-w-0">
        <div className="flex flex-wrap items-center gap-2">
          <h3 className="truncate text-base font-semibold text-zinc-950">
            {restaurant.name}
            {restaurant.branchName ? ` ${restaurant.branchName}` : ""}
          </h3>
          {restaurant.cuisineType ? (
            <span className="rounded bg-zinc-100 px-2 py-0.5 text-xs text-zinc-700">
              {restaurant.cuisineType}
            </span>
          ) : null}
        </div>
        <p className="mt-1 line-clamp-1 text-sm text-zinc-600">{restaurant.address}</p>
        <div className="mt-2 flex flex-wrap gap-3 text-xs text-zinc-500">
          <span>{location || "地區未提供"}</span>
          <span>{price}</span>
          <span>{restaurant.phoneNumber ?? "無電話"}</span>
        </div>
      </div>
      <div className="flex items-center gap-3 sm:justify-end">
        <Metric label="平台分" value={formatScore(restaurant.platformScore)} />
        <Metric label="評論" value={restaurant.reviewCount.toString()} />
        <Metric label="收藏" value={restaurant.favoriteCount.toString()} />
      </div>
    </Link>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="min-w-12 text-center">
      <div className="text-base font-semibold text-zinc-950">{value}</div>
      <div className="text-xs text-zinc-500">{label}</div>
    </div>
  );
}

function formatScore(value: number | null | undefined) {
  return value === null || value === undefined ? "-" : value.toFixed(2);
}
