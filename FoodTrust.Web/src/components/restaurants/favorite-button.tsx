"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { addFavorite, removeFavorite } from "@/lib/api/restaurants";
import {
  isFavoriteRestaurant,
  markFavoriteRestaurant,
  unmarkFavoriteRestaurant,
} from "@/lib/auth/favorite-store";
import { getAccessToken } from "@/lib/auth/token-store";

export function FavoriteButton({ restaurantId }: { restaurantId: number }) {
  const router = useRouter();
  const [active, setActive] = useState(() => isFavoriteRestaurant(restaurantId));
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function toggle() {
    const token = getAccessToken();
    if (!token) {
      setMessage("請先登入會員。");
      return;
    }

    setLoading(true);
    setMessage(null);

    try {
      if (active) {
        await removeFavorite(restaurantId, token);
        unmarkFavoriteRestaurant(restaurantId);
        setActive(false);
        setMessage("已取消收藏。");
      } else {
        await addFavorite(restaurantId, token);
        markFavoriteRestaurant(restaurantId);
        setActive(true);
        setMessage("已加入收藏。");
      }
      router.refresh();
    } catch {
      setMessage("操作失敗，請稍後再試。");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex items-center gap-3">
      <button
        className="h-10 rounded-md bg-zinc-950 px-4 text-sm font-medium text-white disabled:opacity-60"
        disabled={loading}
        onClick={toggle}
        type="button"
      >
        {active ? "取消收藏" : "收藏餐廳"}
      </button>
      {message ? <span className="text-sm text-zinc-600">{message}</span> : null}
    </div>
  );
}
