import { apiFetch, toQueryString } from "@/lib/api/client";
import type { CandidateRestaurantSearchResult } from "@/lib/api/types";

export type CandidateRestaurantSearchParams = {
  status?: string;
  keyword?: string;
  page?: number;
  pageSize?: number;
};

export function searchCandidateRestaurants(
  token: string,
  params: CandidateRestaurantSearchParams,
) {
  return apiFetch<CandidateRestaurantSearchResult>(
    `/api/v1/admin/candidate-restaurants${toQueryString({
      status: params.status ?? "Pending",
      keyword: params.keyword,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    })}`,
    { token },
  );
}

export function approveCandidateRestaurant(
  token: string,
  id: number,
  payload: {
    name: string;
    address: string;
    phoneNumber?: string | null;
  },
) {
  return apiFetch<{ restaurantId: number }>(`/api/v1/admin/candidate-restaurants/${id}/approve`, {
    method: "POST",
    token,
    body: JSON.stringify(payload),
  });
}

export function rejectCandidateRestaurant(token: string, id: number) {
  return apiFetch<void>(`/api/v1/admin/candidate-restaurants/${id}/reject`, {
    method: "POST",
    token,
  });
}
