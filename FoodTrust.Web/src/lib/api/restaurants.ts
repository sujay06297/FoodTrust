import { apiFetch, toQueryString } from "@/lib/api/client";
import type {
  FavoriteRestaurantSearchResult,
  RestaurantDetail,
  RestaurantRankingItem,
  RestaurantReview,
  RestaurantSearchResult,
} from "@/lib/api/types";

export type RestaurantSearchParams = {
  keyword?: string;
  status?: string;
  city?: string;
  district?: string;
  cuisineType?: string;
  priceMin?: string;
  priceMax?: string;
  minScore?: string;
  sortBy?: string;
  page?: string;
  pageSize?: string;
};

export function searchRestaurants(params: RestaurantSearchParams) {
  return apiFetch<RestaurantSearchResult>(
    `/api/v1/restaurants${toQueryString({
      ...params,
      status: params.status ?? "Active",
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    })}`,
  );
}

export function getRestaurantRankings(limit = 8) {
  return apiFetch<RestaurantRankingItem[]>(
    `/api/v1/restaurants/rankings${toQueryString({ limit })}`,
  );
}

export function getRestaurant(id: string | number) {
  return apiFetch<RestaurantDetail>(`/api/v1/restaurants/${id}`);
}

export function getRestaurantReviews(id: string | number, limit = 20) {
  return apiFetch<RestaurantReview[]>(
    `/api/v1/restaurants/${id}/reviews${toQueryString({ limit })}`,
  );
}

export function addFavorite(restaurantId: number, token: string) {
  return apiFetch<void>(`/api/v1/restaurants/${restaurantId}/favorites`, {
    method: "POST",
    token,
  });
}

export function removeFavorite(restaurantId: number, token: string) {
  return apiFetch<void>(`/api/v1/restaurants/${restaurantId}/favorites`, {
    method: "DELETE",
    token,
  });
}

export function getMyFavorites(token: string, page = 1, pageSize = 20) {
  return apiFetch<FavoriteRestaurantSearchResult>(
    `/api/v1/users/me/favorite-restaurants${toQueryString({ page, pageSize })}`,
    { token },
  );
}

export function createReview(
  restaurantId: number,
  token: string,
  payload: {
    tasteScore: number;
    serviceScore: number;
    environmentScore: number;
    valueScore: number;
    revisitScore: number;
    content: string;
    reviewerName?: string | null;
    visitDate?: string | null;
    pricePerPerson?: number | null;
    diningType?: string | null;
    companionType?: string | null;
  },
) {
  return apiFetch<void>(`/api/v1/restaurants/${restaurantId}/reviews`, {
    method: "POST",
    token,
    body: JSON.stringify(payload),
  });
}
