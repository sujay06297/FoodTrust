import Link from "next/link";

type Props = {
  page: number;
  pageSize: number;
  totalCount: number;
  searchParams: Record<string, string | undefined>;
};

export function Pagination({ page, pageSize, totalCount, searchParams }: Props) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const canPrev = page > 1;
  const canNext = page < totalPages;

  return (
    <div className="flex flex-col gap-3 border-t border-zinc-200 px-4 py-3 text-sm text-zinc-600 sm:flex-row sm:items-center sm:justify-between">
      <div>
        第 {page} / {totalPages} 頁
      </div>
      <div className="flex gap-2">
        <PageLink disabled={!canPrev} href={buildHref(searchParams, page - 1)}>
          上一頁
        </PageLink>
        <PageLink disabled={!canNext} href={buildHref(searchParams, page + 1)}>
          下一頁
        </PageLink>
      </div>
    </div>
  );
}

function PageLink({
  children,
  disabled,
  href,
}: {
  children: React.ReactNode;
  disabled: boolean;
  href: string;
}) {
  if (disabled) {
    return (
      <span className="rounded-md border border-zinc-200 px-3 py-2 text-zinc-400">
        {children}
      </span>
    );
  }

  return (
    <Link className="rounded-md border border-zinc-300 px-3 py-2 text-zinc-700 hover:bg-zinc-100" href={href}>
      {children}
    </Link>
  );
}

function buildHref(searchParams: Record<string, string | undefined>, page: number) {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(searchParams)) {
    if (value && key !== "page") {
      params.set(key, value);
    }
  }

  params.set("page", String(page));
  return `/restaurants?${params.toString()}`;
}
