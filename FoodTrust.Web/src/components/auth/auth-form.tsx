"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { login, register } from "@/lib/api/auth";
import { saveAuth } from "@/lib/auth/token-store";

export function AuthForm({ mode }: { mode: "login" | "register" }) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    const form = new FormData(event.currentTarget);
    const email = String(form.get("email") ?? "");
    const password = String(form.get("password") ?? "");
    const displayName = String(form.get("displayName") ?? "");

    try {
      const result =
        mode === "login"
          ? await login(email, password)
          : await register(email, password, displayName);
      saveAuth(result);
      router.push("/restaurants");
    } catch {
      setError(mode === "login" ? "登入失敗，請確認帳密。" : "註冊失敗，請確認資料。");
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={onSubmit} className="mx-auto grid w-full max-w-sm gap-4 border border-zinc-200 bg-white p-6">
      <div>
        <h1 className="text-xl font-semibold text-zinc-950">
          {mode === "login" ? "會員登入" : "建立會員"}
        </h1>
      </div>
      {mode === "register" ? (
        <Field label="暱稱" name="displayName" type="text" autoComplete="name" />
      ) : null}
      <Field label="Email" name="email" type="email" autoComplete="email" />
      <Field label="密碼" name="password" type="password" autoComplete={mode === "login" ? "current-password" : "new-password"} />
      {error ? <p className="text-sm text-red-600">{error}</p> : null}
      <button
        className="h-10 rounded-md bg-zinc-950 px-4 text-sm font-medium text-white disabled:opacity-60"
        disabled={loading}
        type="submit"
      >
        {loading ? "處理中" : mode === "login" ? "登入" : "註冊"}
      </button>
    </form>
  );
}

function Field({
  label,
  name,
  type,
  autoComplete,
}: {
  label: string;
  name: string;
  type: string;
  autoComplete: string;
}) {
  return (
    <label className="grid gap-1 text-sm text-zinc-700">
      <span>{label}</span>
      <input
        className="h-10 rounded-md border border-zinc-300 px-3 outline-none focus:border-zinc-900"
        name={name}
        type={type}
        autoComplete={autoComplete}
        required
      />
    </label>
  );
}
