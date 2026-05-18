import Link from "next/link";

type Props = {
  defaultValues: Record<string, string | undefined>;
};

export function RestaurantSearchForm({ defaultValues }: Props) {
  return (
    <form action="/restaurants" className="grid gap-3 border-b border-zinc-200 bg-white p-4 md:grid-cols-[2fr_1fr_1fr_1fr_auto]">
      <input
        className="h-10 rounded-md border border-zinc-300 px-3 text-sm outline-none focus:border-zinc-900"
        name="keyword"
        placeholder="搜尋餐廳、料理、地址"
        defaultValue={defaultValues.keyword}
      />
      <input
        className="h-10 rounded-md border border-zinc-300 px-3 text-sm outline-none focus:border-zinc-900"
        name="city"
        placeholder="縣市"
        defaultValue={defaultValues.city}
      />
      <input
        className="h-10 rounded-md border border-zinc-300 px-3 text-sm outline-none focus:border-zinc-900"
        name="cuisineType"
        placeholder="料理類型"
        defaultValue={defaultValues.cuisineType}
      />
      <select
        className="h-10 rounded-md border border-zinc-300 px-3 text-sm outline-none focus:border-zinc-900"
        name="sortBy"
        defaultValue={defaultValues.sortBy ?? "ranking"}
      >
        <option value="ranking">綜合排行</option>
        <option value="reviewCount">評論最多</option>
        <option value="favoriteCount">收藏最多</option>
        <option value="latest">最新建立</option>
      </select>
      <div className="flex gap-2">
        <button className="h-10 rounded-md bg-zinc-950 px-4 text-sm font-medium text-white" type="submit">
          搜尋
        </button>
        <Link className="h-10 rounded-md border border-zinc-300 px-4 py-2 text-sm text-zinc-700" href="/restaurants">
          清除
        </Link>
      </div>
    </form>
  );
}
