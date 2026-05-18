import Link from "next/link";

export function SiteHeader() {
  return (
    <header className="border-b border-zinc-200 bg-white">
      <div className="mx-auto flex h-14 max-w-6xl items-center justify-between px-4">
        <Link href="/" className="text-lg font-semibold text-zinc-950">
          FoodTrust 食信
        </Link>
        <nav className="flex items-center gap-1 text-sm">
          <Link className="rounded-md px-3 py-2 text-zinc-700 hover:bg-zinc-100" href="/restaurants">
            餐廳
          </Link>
          <Link className="rounded-md px-3 py-2 text-zinc-700 hover:bg-zinc-100" href="/me/favorites">
            收藏
          </Link>
          <Link className="rounded-md px-3 py-2 text-zinc-700 hover:bg-zinc-100" href="/login">
            登入
          </Link>
        </nav>
      </div>
    </header>
  );
}
