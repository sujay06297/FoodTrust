"use client";

import { useState } from "react";
import Link from "next/link";
import type { UserAuthResult } from "@/lib/api/types";
import { clearAuth, loadAuth } from "@/lib/auth/token-store";

export function AuthStatus() {
  const [auth, setAuth] = useState<UserAuthResult | null>(() => loadAuth());

  function logout() {
    clearAuth();
    setAuth(null);
    window.location.href = "/";
  }

  if (!auth) {
    return (
      <Link className="rounded-md px-3 py-2 text-zinc-700 hover:bg-zinc-100" href="/login">
        登入
      </Link>
    );
  }

  return (
    <div className="flex items-center gap-2">
      <span className="hidden max-w-28 truncate text-zinc-600 sm:inline">{auth.user.displayName}</span>
      <button className="rounded-md px-3 py-2 text-zinc-700 hover:bg-zinc-100" onClick={logout} type="button">
        登出
      </button>
    </div>
  );
}
