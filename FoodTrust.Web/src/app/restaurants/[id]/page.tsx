import { FavoriteButton } from "@/components/restaurants/favorite-button";
import { ReviewForm } from "@/components/restaurants/review-form";
import { getRestaurant, getRestaurantReviews } from "@/lib/api/restaurants";

type PageProps = {
  params: Promise<{ id: string }>;
};

export default async function RestaurantDetailPage({ params }: PageProps) {
  const { id } = await params;
  const [restaurant, reviews] = await Promise.all([
    getRestaurant(id).catch(() => null),
    getRestaurantReviews(id).catch(() => []),
  ]);

  if (!restaurant) {
    return (
      <main className="mx-auto max-w-4xl px-4 py-6">
        <p className="border border-zinc-200 bg-white p-4 text-sm text-zinc-600">
          找不到餐廳或 API 尚未啟動。
        </p>
      </main>
    );
  }

  return (
    <main className="mx-auto grid max-w-4xl gap-4 px-4 py-6">
      <section className="border border-zinc-200 bg-white p-5">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-zinc-950">
              {restaurant.name}
              {restaurant.branchName ? ` ${restaurant.branchName}` : ""}
            </h1>
            <p className="mt-2 text-sm text-zinc-600">{restaurant.address}</p>
            <div className="mt-3 flex flex-wrap gap-2 text-xs text-zinc-600">
              {restaurant.city ? <span className="rounded bg-zinc-100 px-2 py-1">{restaurant.city}</span> : null}
              {restaurant.district ? <span className="rounded bg-zinc-100 px-2 py-1">{restaurant.district}</span> : null}
              {restaurant.cuisineType ? <span className="rounded bg-zinc-100 px-2 py-1">{restaurant.cuisineType}</span> : null}
              {restaurant.priceMin || restaurant.priceMax ? (
                <span className="rounded bg-zinc-100 px-2 py-1">
                  ${restaurant.priceMin ?? "?"} - ${restaurant.priceMax ?? "?"}
                </span>
              ) : null}
            </div>
          </div>
          <FavoriteButton restaurantId={restaurant.id} />
        </div>
        {restaurant.description ? (
          <p className="mt-4 whitespace-pre-line text-sm leading-6 text-zinc-700">{restaurant.description}</p>
        ) : null}
      </section>

      <section className="border border-zinc-200 bg-white">
        <div className="border-b border-zinc-200 px-4 py-3">
          <h2 className="font-semibold text-zinc-950">評論</h2>
        </div>
        {reviews.length > 0 ? (
          <div>
            {reviews.map((review) => (
              <article key={review.id} className="border-b border-zinc-200 p-4">
                <div className="flex flex-wrap items-center gap-3">
                  <span className="font-semibold text-zinc-950">{review.averageScore.toFixed(1)}</span>
                  <span className="text-sm text-zinc-600">{review.reviewerName ?? "會員"}</span>
                  <span className="text-xs text-zinc-500">{new Date(review.createdAt).toLocaleDateString("zh-TW")}</span>
                </div>
                <p className="mt-2 whitespace-pre-line text-sm leading-6 text-zinc-700">{review.content}</p>
              </article>
            ))}
          </div>
        ) : (
          <p className="p-4 text-sm text-zinc-600">尚無公開評論。</p>
        )}
      </section>

      <ReviewForm restaurantId={restaurant.id} />
    </main>
  );
}
