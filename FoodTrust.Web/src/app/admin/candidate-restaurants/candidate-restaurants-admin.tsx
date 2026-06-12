"use client";

import { useEffect, useMemo, useState } from "react";
import {
  approveCandidateRestaurant,
  rejectCandidateRestaurant,
  searchCandidateRestaurants,
} from "@/lib/api/admin";
import { getApiErrorMessage } from "@/lib/api/client";
import type { CandidateRestaurant } from "@/lib/api/types";

const tokenStorageKey = "foodtrust.admin.token";

type Draft = {
  name: string;
  address: string;
  phoneNumber: string;
};

export function CandidateRestaurantsAdmin() {
  const [token, setToken] = useState("");
  const [keyword, setKeyword] = useState("");
  const [status, setStatus] = useState("Pending");
  const [page, setPage] = useState(1);
  const [items, setItems] = useState<CandidateRestaurant[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [drafts, setDrafts] = useState<Record<number, Draft>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / 20)), [totalCount]);

  useEffect(() => {
    setToken(localStorage.getItem(tokenStorageKey) ?? "");
  }, []);

  async function loadCandidates(nextPage = page) {
    if (!token.trim()) {
      setMessage("Enter the admin access token first.");
      return;
    }

    setIsLoading(true);
    setMessage(null);

    try {
      const result = await searchCandidateRestaurants(token.trim(), {
        status,
        keyword,
        page: nextPage,
        pageSize: 20,
      });
      setItems(result.items);
      setTotalCount(result.totalCount);
      setPage(result.page);
      setDrafts(Object.fromEntries(result.items.map((item) => [item.id, toDraft(item)])));
    } catch (error) {
      setMessage(getApiErrorMessage(error, "Failed to load candidate restaurants."));
    } finally {
      setIsLoading(false);
    }
  }

  async function approve(item: CandidateRestaurant) {
    const draft = drafts[item.id] ?? toDraft(item);
    if (!draft.name.trim() || !draft.address.trim()) {
      setMessage("Name and address are required before approving.");
      return;
    }

    setIsLoading(true);
    setMessage(null);

    try {
      const result = await approveCandidateRestaurant(token.trim(), item.id, {
        name: draft.name.trim(),
        address: draft.address.trim(),
        phoneNumber: draft.phoneNumber.trim() || null,
      });
      setMessage(`Approved and inserted into restaurants. ID: ${result.restaurantId}`);
      await loadCandidates(page);
    } catch (error) {
      setMessage(getApiErrorMessage(error, "Failed to approve candidate restaurant."));
    } finally {
      setIsLoading(false);
    }
  }

  async function reject(item: CandidateRestaurant) {
    setIsLoading(true);
    setMessage(null);

    try {
      await rejectCandidateRestaurant(token.trim(), item.id);
      setMessage("Candidate restaurant rejected.");
      await loadCandidates(page);
    } catch (error) {
      setMessage(getApiErrorMessage(error, "Failed to reject candidate restaurant."));
    } finally {
      setIsLoading(false);
    }
  }

  function saveToken() {
    localStorage.setItem(tokenStorageKey, token.trim());
    setMessage("Admin token saved.");
  }

  function updateDraft(id: number, patch: Partial<Draft>) {
    setDrafts((current) => ({
      ...current,
      [id]: {
        ...(current[id] ?? { name: "", address: "", phoneNumber: "" }),
        ...patch,
      },
    }));
  }

  return (
    <main className="mx-auto max-w-6xl px-4 py-8">
      <div className="mb-6 flex flex-col gap-4 border-b border-zinc-200 pb-5">
        <div>
          <h1 className="text-2xl font-semibold text-zinc-950">Candidate Restaurants</h1>
          <p className="mt-1 text-sm text-zinc-600">
            Review imported food business records, search Google manually, then approve real
            restaurants into the restaurants table.
          </p>
        </div>

        <div className="grid gap-3 md:grid-cols-[1fr_auto]">
          <input
            className="h-10 rounded-md border border-zinc-300 px-3 text-sm"
            placeholder="Admin access token"
            type="password"
            value={token}
            onChange={(event) => setToken(event.target.value)}
          />
          <button
            className="h-10 rounded-md bg-zinc-950 px-4 text-sm font-medium text-white disabled:bg-zinc-400"
            onClick={saveToken}
            type="button"
          >
            Save Token
          </button>
        </div>

        <div className="grid gap-3 md:grid-cols-[160px_1fr_auto]">
          <select
            className="h-10 rounded-md border border-zinc-300 px-3 text-sm"
            value={status}
            onChange={(event) => {
              setStatus(event.target.value);
              setPage(1);
            }}
          >
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
          <input
            className="h-10 rounded-md border border-zinc-300 px-3 text-sm"
            placeholder="Search name, address, registration number"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
          />
          <button
            className="h-10 rounded-md bg-zinc-900 px-4 text-sm font-medium text-white disabled:bg-zinc-400"
            disabled={isLoading}
            onClick={() => loadCandidates(1)}
            type="button"
          >
            Search
          </button>
        </div>

        {message ? <p className="text-sm text-zinc-700">{message}</p> : null}
      </div>

      <div className="space-y-3">
        {items.map((item) => {
          const draft = drafts[item.id] ?? toDraft(item);
          const googleSearchUrl = buildGoogleSearchUrl(item, draft);

          return (
            <section key={item.id} className="rounded-md border border-zinc-200 bg-white p-4">
              <div className="mb-3 flex flex-wrap items-center justify-between gap-2 text-sm">
                <div className="font-medium text-zinc-950">
                  #{item.id} {item.rawName}
                </div>
                <div className="text-zinc-500">{item.sourceKey}</div>
              </div>

              <div className="grid gap-3 md:grid-cols-3">
                <label className="text-sm">
                  <span className="mb-1 block text-zinc-600">Restaurant name</span>
                  <input
                    className="h-10 w-full rounded-md border border-zinc-300 px-3"
                    value={draft.name}
                    onChange={(event) => updateDraft(item.id, { name: event.target.value })}
                  />
                </label>
                <label className="text-sm md:col-span-2">
                  <span className="mb-1 block text-zinc-600">Address</span>
                  <input
                    className="h-10 w-full rounded-md border border-zinc-300 px-3"
                    value={draft.address}
                    onChange={(event) => updateDraft(item.id, { address: event.target.value })}
                  />
                </label>
                <label className="text-sm">
                  <span className="mb-1 block text-zinc-600">Phone</span>
                  <input
                    className="h-10 w-full rounded-md border border-zinc-300 px-3"
                    value={draft.phoneNumber}
                    onChange={(event) => updateDraft(item.id, { phoneNumber: event.target.value })}
                  />
                </label>
                <div className="text-sm md:col-span-2">
                  <span className="mb-1 block text-zinc-600">Raw address</span>
                  <div className="rounded-md bg-zinc-50 px-3 py-2 text-zinc-800">{item.rawAddress}</div>
                </div>
              </div>

              <div className="mt-4 flex flex-wrap justify-end gap-2">
                {item.linkedRestaurantId ? (
                  <span className="rounded-md bg-zinc-100 px-3 py-2 text-sm text-zinc-700">
                    Restaurant ID {item.linkedRestaurantId}
                  </span>
                ) : null}
                <a
                  className="rounded-md border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-800 hover:bg-zinc-50"
                  href={googleSearchUrl}
                  rel="noreferrer"
                  target="_blank"
                >
                  Google Search
                </a>
                <button
                  className="rounded-md border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-800 disabled:text-zinc-400"
                  disabled={isLoading || item.status !== "Pending"}
                  onClick={() => reject(item)}
                  type="button"
                >
                  Reject
                </button>
                <button
                  className="rounded-md bg-zinc-950 px-4 py-2 text-sm font-medium text-white disabled:bg-zinc-400"
                  disabled={isLoading || item.status !== "Pending"}
                  onClick={() => approve(item)}
                  type="button"
                >
                  Approve
                </button>
              </div>
            </section>
          );
        })}
      </div>

      <div className="mt-6 flex items-center justify-between text-sm text-zinc-600">
        <span>Total {totalCount}</span>
        <div className="flex items-center gap-2">
          <button
            className="rounded-md border border-zinc-300 px-3 py-2 disabled:text-zinc-400"
            disabled={isLoading || page <= 1}
            onClick={() => loadCandidates(page - 1)}
            type="button"
          >
            Previous
          </button>
          <span>
            {page} / {totalPages}
          </span>
          <button
            className="rounded-md border border-zinc-300 px-3 py-2 disabled:text-zinc-400"
            disabled={isLoading || page >= totalPages}
            onClick={() => loadCandidates(page + 1)}
            type="button"
          >
            Next
          </button>
        </div>
      </div>
    </main>
  );
}

function toDraft(item: CandidateRestaurant): Draft {
  return {
    name: item.suggestedName ?? "",
    address: item.rawAddress,
    phoneNumber: item.rawPhoneNumber ?? "",
  };
}

function buildGoogleSearchUrl(item: CandidateRestaurant, draft: Draft) {
  const query = [draft.name || item.rawName, item.rawAddress]
    .filter((value) => value.trim().length > 0)
    .join(" ");
  return `https://www.google.com/search?q=${encodeURIComponent(query)}`;
}
