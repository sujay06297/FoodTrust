# ADR 0002：整理為 RESTful API + Clean Architecture + DDD

- 狀態：已採用（2026-07-21）
- 相關：[../devlog.md](../devlog.md) 2026-07-21 項目、[../handoff.md](../handoff.md)

## 背景

後端在多個功能 slice 疊加後，路由命名混用「動作型」（如 `/api/v1/auth/login`、`/favorite`）與「資源型」，業務規則也分散在 controller、repository 與 service 的 primitive validation 裡，缺乏一致的 domain 邊界。需要一次性整理，讓 API 符合 RESTful 慣例、分層符合 Clean Architecture、業務規則收斂到 DDD 的 domain model / value object。

## 決策

### RESTful API 調整

| 舊路由 | 新路由 |
| --- | --- |
| `POST /api/v1/auth/register` | `POST /api/v1/users` |
| `POST /api/v1/auth/login` | `POST /api/v1/sessions` |
| `POST /api/v1/admin/auth/login` | `POST /api/v1/admin/sessions` |
| 管理員 refresh token 交換 | `POST /api/v1/admin/refresh-tokens/exchanges` |
| 管理員 refresh token 撤銷 | `DELETE /api/v1/admin/refresh-tokens` |
| 餐廳收藏 `/favorite`（單數動作路由） | `/favorites`（集合資源） |
| 候選餐廳 approve/reject 動作路由 | `PATCH /api/v1/admin/candidate-restaurants/{id}/status` |

前端 `FoodTrust.Web/src/lib/api/*` 已同步更新上述 endpoint。

### Clean Architecture 分層（維持並加強）

- `FoodTrust.Core`：內層，domain model、value object、interface、application service。
- `FoodTrust.Infrastructure`：外層，Dapper/MySQL repository、migration、外部匯入來源、安全雜湊（實作 Core 定義的 interface）。
- `FoodTrust.Api`：delivery/composition root，controller、request model、JWT、DI 組裝；已明確加入 `FoodTrust.Core` 專案參考，讓依賴方向清楚化。
- `FoodTrust.Worker`：繼續透過 Core/Infrastructure 執行背景匯入流程。

### DDD 收斂

新增的 domain / value object 分佈：

- `FoodTrust.Core/Common/Domain`：`EntityId`、`PageRequest`、`OptionalText`。
- `FoodTrust.Core/Users/Domain/ValueObjects`：`UserEmail`、`DisplayName`、`AccountPassword`。
- `FoodTrust.Core/Admin/Domain/ValueObjects`：`AdminUsername`、`AdminDisplayName`、`AdminRoleName`。
- `FoodTrust.Core/Restaurants/Domain`：`Restaurant` aggregate、`RestaurantReview`、`FavoriteRestaurant`，及對應 ValueObjects（`RestaurantName`、`RestaurantAddress`、`PriceRange`、`RestaurantLifecycleStatus`、`ReviewScore`、`ReviewContent`、`PricePerPerson`、`RestaurantReviewStatusName`、`ReviewReportReason`、`ReviewReportStatusName`、`ModerationActionName`）。
- `FoodTrust.Core/RestaurantImports/Domain`：`ImportBatchSize`、`CandidateRestaurantLifecycleStatus`。

`UserAuthService`、`AdminAuthService`、`AdminUserService`、`RestaurantService`、`RestaurantReviewService`、`RestaurantFavoriteService`、`CandidateRestaurantService`、`RestaurantImportService` 改為透過 domain/value object 執行業務規則驗證與流程協調，取代原本分散在 controller/repository/service 的 primitive validation（例如 Email 格式、密碼長度、餐廳名稱與地址、價格區間、評論分數、評論內容長度、檢舉原因、管理員角色、收藏識別碼、匯入批次大小）。

## 後果

- API 路由是 breaking change，任何外部呼叫方（含 `FoodTrust.Web`）都需要同步更新；本次已同步更新前端 API client。
- 規格書 13. 節「API 規格範例」的舊範例（`POST /api/restaurants/{restaurantId}/reviews` 等）保留作為設計意圖參考，已加註說明實際路由已調整，避免讀者誤把範例當成目前真實路由。
- 後續新增的業務規則應優先考慮放進 domain/value object，而不是回頭加在 controller 或 repository 的 primitive validation。
- 驗證結果：`dotnet build FoodTrust.Api\FoodTrust.Api.csproj`、`dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj`、`npm run build`（FoodTrust.Web）均通過；API build 仍有 `Microsoft.OpenApi 2.3.0` high severity 套件警告待後續升級。
