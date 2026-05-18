"use client";

import { loadAuth } from "@/lib/auth/token-store";

function getStorageKey() {
  const userId = loadAuth()?.user.id;
  return userId ? `foodtrust.favoriteRestaurantIds.${userId}` : null;
}

export function loadFavoriteRestaurantIds() {
  const key = getStorageKey();
  if (!key || typeof window === "undefined") {
    return new Set<number>();
  }

  const value = localStorage.getItem(key);
  if (!value) {
    return new Set<number>();
  }

  try {
    const ids = JSON.parse(value) as number[];
    return new Set(ids.filter(Number.isFinite));
  } catch {
    localStorage.removeItem(key);
    return new Set<number>();
  }
}

export function isFavoriteRestaurant(restaurantId: number) {
  return loadFavoriteRestaurantIds().has(restaurantId);
}

export function rememberFavoriteRestaurants(restaurantIds: number[]) {
  const ids = loadFavoriteRestaurantIds();
  for (const restaurantId of restaurantIds) {
    ids.add(restaurantId);
  }
  saveFavoriteRestaurantIds(ids);
}

export function markFavoriteRestaurant(restaurantId: number) {
  const ids = loadFavoriteRestaurantIds();
  ids.add(restaurantId);
  saveFavoriteRestaurantIds(ids);
}

export function unmarkFavoriteRestaurant(restaurantId: number) {
  const ids = loadFavoriteRestaurantIds();
  ids.delete(restaurantId);
  saveFavoriteRestaurantIds(ids);
}

function saveFavoriteRestaurantIds(ids: Set<number>) {
  const key = getStorageKey();
  if (!key || typeof window === "undefined") {
    return;
  }

  localStorage.setItem(key, JSON.stringify([...ids]));
}
