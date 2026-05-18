"use client";

import type { UserAuthResult } from "@/lib/api/types";

const storageKey = "foodtrust.auth";

export function saveAuth(result: UserAuthResult) {
  if (typeof window === "undefined") {
    return;
  }

  localStorage.setItem(storageKey, JSON.stringify(result));
}

export function loadAuth(): UserAuthResult | null {
  if (typeof window === "undefined") {
    return null;
  }

  const value = localStorage.getItem(storageKey);
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as UserAuthResult;
  } catch {
    localStorage.removeItem(storageKey);
    return null;
  }
}

export function getAccessToken() {
  return loadAuth()?.accessToken ?? null;
}

export function clearAuth() {
  if (typeof window === "undefined") {
    return;
  }

  localStorage.removeItem(storageKey);
}
