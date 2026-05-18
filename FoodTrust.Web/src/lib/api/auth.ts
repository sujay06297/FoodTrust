import { apiFetch } from "@/lib/api/client";
import type { UserAuthResult } from "@/lib/api/types";

export function login(email: string, password: string) {
  return apiFetch<UserAuthResult>("/api/v1/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export function register(email: string, password: string, displayName: string) {
  return apiFetch<UserAuthResult>("/api/v1/auth/register", {
    method: "POST",
    body: JSON.stringify({ email, password, displayName }),
  });
}
