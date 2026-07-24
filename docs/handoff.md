# 接續狀態快照（Handoff）

> 給新視窗/新對話的助理：先讀這份檔案掌握現況，需要更早期的來龍去脈再查 [devlog.md](devlog.md)。
>
> 這份文件只保留「現在的狀態」，不要在這裡疊加日期段落——歷史紀錄一律寫進 [devlog.md](devlog.md)。每次更新時直接覆寫對應小節即可。
>
> 最後更新：2026-07-24（產品/部署狀態依 2026-07-21 架構調整紀錄整理；2026-07-23/07-24 做了文件結構與規範整理，見 [devlog.md](devlog.md) 對應日期項目）

## 專案位置與分層

工作目錄：`C:\Users\User\FoodTrust`。方案包含 `FoodTrust.Api`、`FoodTrust.Core`、`FoodTrust.Infrastructure`、`FoodTrust.Worker`、`FoodTrust.Web`。

Clean Architecture 分層原則：

- `FoodTrust.Core`：domain/application 介面、模型、value object 與 service（內層）。
- `FoodTrust.Infrastructure`：Dapper/MySQL repository、migration、外部匯入（外層，實作 Core 的 interface 是正常分層）。
- `FoodTrust.Api`：Controllers、request model、JWT 設定、DI 組裝（composition root，已明確加入 `FoodTrust.Core` 專案參考）。
- `FoodTrust.Worker`：背景匯入（透過 Core/Infrastructure）。
- `FoodTrust.Web`：Next.js App Router + TypeScript + Tailwind 前台。

資料庫使用 MySQL 相容的 TiDB Cloud Starter；migration 由 `FoodTrust.Infrastructure\Data\DatabaseMigrations.cs` 與 `DatabaseInitializer` 啟動時套用，`schema_migrations` 記錄版本。

DDD 收斂現況：`FoodTrust.Core/Common/Domain`（EntityId、PageRequest、OptionalText）、`Users/Domain/ValueObjects`、`Admin/Domain/ValueObjects`、`Restaurants/Domain`（Restaurant aggregate、RestaurantReview、FavoriteRestaurant 及其 ValueObjects）、`RestaurantImports/Domain` 已建立，主要 service 已改為透過 domain/value object 做業務規則驗證。

RESTful 路由（2026-07-21 調整後）：`POST /api/v1/users`（註冊）、`POST /api/v1/sessions`（會員登入）、`POST /api/v1/admin/sessions`（管理員登入）、`POST /api/v1/admin/refresh-tokens/exchanges`、`DELETE /api/v1/admin/refresh-tokens`、`/api/v1/.../favorites`（集合資源）、`PATCH /api/v1/admin/candidate-restaurants/{id}/status`。

## 已完成功能

- 餐廳 CRUD、查詢、詳細資料、狀態更新。
- 餐廳搜尋：keyword、status、city、district、cuisineType、priceMin、priceMax、minScore、page、pageSize；排序支援 latest、ranking、reviewCount、favoriteCount。
- 排行/列表分數：Bayesian 平台分數（僅計 approved / non-suspicious / non-deleted 評論），已納入收藏數 FavoriteScore（5% 權重）。
- 評論：五維分數（Taste/Service/Environment/Value/Revisit）、平均分、內容、用餐日期、人均消費、用餐型態、同行型態、審核狀態、可疑/刪除旗標；同會員同餐廳 30 天內限一次有效評論；匿名評論檢舉。
- 反作弊 MVP（規則式）：同評論者短時間大量評論、同餐廳重複內容、低品質內容、與既有餐廳平均分差距過大，命中後自動標記 `is_suspicious` 並保存 `suspicious_reason`、`suspicious_detected_at`。
- 台灣 FDA 食品業者資料匯入 → 先進候選表 `candidate_restaurants`、人工審核通過才寫入 `restaurants`；ZIP/JSON/CSV 解析、餐飲業者篩選、來源 key 去重、同名同地址避免重複建立；後台 `/admin/candidate-restaurants` 頁面含 Approve/Reject 與 Google 搜尋按鈕。`.NET Worker` 支援定期匯入、啟動時匯入、匯入紀錄查詢。
- 會員系統：註冊/登入、PBKDF2 密碼雜湊、User JWT；登入會員新增評論會從 JWT 寫入 `user_id`。
- 收藏功能：`favorite_restaurants` 資料表，會員收藏/取消收藏、我的收藏分頁列表，前台收藏狀態同步。
- 後台管理：第一位管理員 bootstrap、PBKDF2、Admin JWT、Admin Refresh Token 輪替/撤銷、Admin 角色 + ReviewModerator 角色細分（`Admin.ReviewModeration` policy）、管理員列表/啟用停用/改密碼/角色更新。
- 評論審核：列表查詢、單筆/批次狀態更新、可疑標記、刪除標記、審核原因紀錄、操作紀錄查詢（含全域搜尋）。
- 檢舉管理：後台檢舉列表、狀態處理、處理管理員與備註。
- `FoodTrust.Web` 前台：首頁、餐廳搜尋列表（含分頁）、餐廳詳細、登入/註冊、會員狀態列/登出、我的收藏、收藏按鈕（含狀態同步）、新增評論表單（含送出後刷新）、API 錯誤訊息解析。
- SEO：頁面 metadata、餐廳詳情動態 metadata、`robots.txt`、`sitemap.xml`。
- 前端工程化：`.env.example`、`npm run typecheck` / `verify`、GitHub Actions CI（lint/typecheck/build）、`amplify.yml`（monorepo，目前非採用中的部署方式，見下方阻塞）。

## 尚未完成

照片上傳、地圖/距離搜尋、Typesense 搜尋、Redis 快取、商家認領、SEO 文章頁、獨立城市/行政區/料理分類資料表、會員 Refresh Token 與 email 驗證、反作弊 IP/裝置指紋與會員信任分數、批次重算。

## 目前部署狀態與阻塞（2026-06-12 起，尚未解除）

雲端部署決策（2026-06-10）：前端 Vercel、API/Worker Google Cloud Run（Worker 規劃改 Cloud Run Jobs + Cloud Scheduler）、資料庫 TiDB Cloud Starter。AWS Amplify 已停用/規劃退場，退場清單見 [ops/aws-teardown.md](ops/aws-teardown.md)。

GCP 現況：專案 `foodtrust-dev`、Artifact Registry `foodtrust`（區域 asia-east1）、必要 API（Cloud Run Admin、Artifact Registry、Secret Manager、Cloud Scheduler、Cloud Build）已啟用；Cloud Build 已能把 API image 推到 `asia-east1-docker.pkg.dev/foodtrust-dev/foodtrust/foodtrust-api:dev`；API 已補 CORS（預設允許 `http://localhost:3000` / `http://127.0.0.1:3000`）。

**阻塞**：Cloud Run `foodtrust-api` 首次 deploy 失敗，容器未在 `PORT=8080` 上啟動。高機率根因：啟動時執行 `DatabaseInitializer.InitializeAsync()`，但 Cloud Run 尚未設定正確的 TiDB connection string / JWT signing key / CORS 正式設定，導致容器在 listen 前 crash。

**下一步（依序）**：

1. 用最新版 image（含 CORS）重跑 `gcloud builds submit --config cloudbuild.api.yaml .`，確認 `foodtrust-api:dev` 是最新版本。
2. 建立 Secret Manager secrets：至少 TiDB connection string、AdminJwt signing key、UserJwt signing key。
3. 用 `gcloud run deploy ... --set-secrets ...` 重新部署 `foodtrust-api`，確認 Cloud Run URL 可對外提供 API。
4. 把 `FoodTrust.Web/.env.local` 的 `NEXT_PUBLIC_API_BASE_URL` 改成 Cloud Run DEV URL，驗證本機前端直接打 DEV API。
5. 評估 `FoodTrust.Worker` 改為 Cloud Run Jobs + Cloud Scheduler，取代目前長駐 worker。
6. 產品面持續推進：照片上傳、地圖/距離搜尋、Typesense、Redis、SEO 文章頁、商家認領；反作弊持續補強 IP/裝置指紋、會員信任分數、人工審核回饋與批次重算。

## 建置與操作注意事項

- Windows 下請用單執行緒建置避免 DLL lock：
  `dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1`
  `dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1`
- Git 可能不在 PATH；找不到時檢查是否有 Fork 內建的 `git.exe`（例如 `AppData\Local\Fork\gitInstance\<版本>\cmd\git.exe`）。
- PowerShell `Get-Content` 讀取 UTF-8 中文檔可能顯示亂碼，不代表檔案壞掉；改用 `rg` 讀取或明確指定 UTF-8。
- 近期 commit 內容請直接查 `git log` / `git status`，不要在文件裡手動抄一份（會過時）。

## 文件維護規則

每完成一個功能 slice：更新本檔對應小節（已完成 / 尚未完成 / 阻塞 / 下一步）→ build Api/Worker → commit。規格本體異動才動 [FoodTrust_project_spec.md](../FoodTrust_project_spec.md)；有意義的歷史事件（架構決策、部署里程碑）才寫進 [devlog.md](devlog.md)，用日期分段、只增不改。使用者偏好中文註解與中文說明。

已知套件警告：API build 有 `Microsoft.OpenApi 2.3.0` high severity vulnerability，待升級 Swashbuckle/OpenAPI 相關套件。
