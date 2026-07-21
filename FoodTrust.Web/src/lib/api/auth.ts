import { apiFetch } from "@/lib/api/client";
import type { UserAuthResult } from "@/lib/api/types";

export function login(email: string, password: string) {
  return apiFetch<UserAuthResult>("/api/v1/sessions", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export function register(email: string, password: string, displayName: string) {
  return apiFetch<UserAuthResult>("/api/v1/users", {
    method: "POST",
    body: JSON.stringify({ email, password, displayName }),
  });
}
