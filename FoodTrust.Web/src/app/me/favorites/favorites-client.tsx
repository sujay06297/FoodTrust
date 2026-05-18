"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { getApiErrorMessage } from "@/lib/api/client";
import { getMyFavorites } from "@/lib/api/restaurants";
import type { FavoriteRestaurantSearchResult } from "@/lib/api/types";
import { rememberFavoriteRestaurants } from "@/lib/auth/favorite-store";
import { getAccessToken, loadAuth } from "@/lib/auth/token-store";

export function FavoritesClient() {
  const [hasToken] = useState(() => Boolean(loadAuth()?.accessToken));
  const [result, setResult] = useState<FavoriteRestaurantSearchResult | null>(null);
  const [message, setMessage] = useState(hasToken ? "載入中" : "請先登入會員。");

  useEffect(() => {
    const token = getAccessToken();
    if (!token) {
      return;
    }

    getMyFavorites(token)
      .then((data) => {
        rememberFavoriteRestaurants(data.items.map((item) => item.restaurantId ?? item.id));
        setResult(data);
        setMessage("");
      })
      .catch((error) => setMessage(getApiErrorMessage(error, "無法載入收藏。")));
  }, []);

  if (message) {
    return <p className="border border-zinc-200 bg-white p-4 text-sm text-zinc-600">{message}</p>;
  }

  return (
    <section className="border border-zinc-200 bg-white">
      {result?.items.map((item) => {
        const id = item.restaurantId ?? item.id;
        return (
          <Link key={id} href={`/restaurants/${id}`} className="block border-b border-zinc-200 p-4 hover:bg-zinc-50">
            <div className="font-semibold text-zinc-950">{item.name}</div>
            <div className="mt-1 text-sm text-zinc-600">{item.address}</div>
          </Link>
        );
      })}
      {result?.items.length === 0 ? <p className="p-4 text-sm text-zinc-600">尚未收藏餐廳。</p> : null}
    </section>
  );
}
