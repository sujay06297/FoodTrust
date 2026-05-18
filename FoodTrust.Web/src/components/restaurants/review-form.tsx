"use client";

import { FormEvent, useState } from "react";
import { createReview } from "@/lib/api/restaurants";
import { getAccessToken } from "@/lib/auth/token-store";

export function ReviewForm({ restaurantId }: { restaurantId: number }) {
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const token = getAccessToken();
    if (!token) {
      setMessage("請先登入會員。");
      return;
    }

    const form = new FormData(event.currentTarget);
    setLoading(true);
    setMessage(null);

    try {
      await createReview(restaurantId, token, {
        tasteScore: Number(form.get("tasteScore")),
        serviceScore: Number(form.get("serviceScore")),
        environmentScore: Number(form.get("environmentScore")),
        valueScore: Number(form.get("valueScore")),
        revisitScore: Number(form.get("revisitScore")),
        content: String(form.get("content") ?? ""),
        pricePerPerson: form.get("pricePerPerson") ? Number(form.get("pricePerPerson")) : null,
      });
      event.currentTarget.reset();
      setMessage("評論已送出。");
    } catch {
      setMessage("評論送出失敗，請確認內容至少 30 字且 30 天內未重複評論。");
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={onSubmit} className="grid gap-3 border border-zinc-200 bg-white p-4">
      <h2 className="text-lg font-semibold text-zinc-950">撰寫評論</h2>
      <div className="grid gap-3 sm:grid-cols-5">
        <ScoreField name="tasteScore" label="味道" />
        <ScoreField name="serviceScore" label="服務" />
        <ScoreField name="environmentScore" label="環境" />
        <ScoreField name="valueScore" label="CP" />
        <ScoreField name="revisitScore" label="再訪" />
      </div>
      <input
        className="h-10 rounded-md border border-zinc-300 px-3 text-sm outline-none focus:border-zinc-900"
        name="pricePerPerson"
        placeholder="人均消費"
        type="number"
        min="0"
      />
      <textarea
        className="min-h-28 rounded-md border border-zinc-300 px-3 py-2 text-sm outline-none focus:border-zinc-900"
        name="content"
        placeholder="分享你的用餐心得，至少 30 字"
        required
      />
      {message ? <p className="text-sm text-zinc-600">{message}</p> : null}
      <button
        className="h-10 w-fit rounded-md bg-zinc-950 px-4 text-sm font-medium text-white disabled:opacity-60"
        disabled={loading}
        type="submit"
      >
        {loading ? "送出中" : "送出評論"}
      </button>
    </form>
  );
}

function ScoreField({ name, label }: { name: string; label: string }) {
  return (
    <label className="grid gap-1 text-sm text-zinc-700">
      <span>{label}</span>
      <input
        className="h-10 rounded-md border border-zinc-300 px-2 outline-none focus:border-zinc-900"
        name={name}
        type="number"
        min="1"
        max="5"
        step="0.1"
        defaultValue="4"
        required
      />
    </label>
  );
}
