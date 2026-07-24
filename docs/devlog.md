# 開發歷史紀錄（Devlog）

> 只增不改：每次有意義的進度、決策或部署里程碑，往最下面加一個日期段落即可，不要回頭改寫舊段落。
>
> 現在的專案狀態（不需要爬歷史就能懂）請看 [handoff.md](handoff.md)；產品規格本體看 [FoodTrust_project_spec.md](../FoodTrust_project_spec.md)。

## 2026-05-15：後端 MVP 第一批落地

- 完成：ASP.NET Core API 專案、Core/Infrastructure 分層、MySQL 初始化、餐廳資料表、餐廳來源表、餐廳匯入紀錄表、餐廳評分表。
- 完成：餐廳新增、查詢、詳細資料、更新、狀態更新 API。
- 完成：簡易評分 API（1-5 分）、簡易排行榜 API（依平均分數與評分數排序）。
- 完成：台灣 FDA 食品業者資料匯入、ZIP/JSON/CSV 解析、餐飲業者篩選、來源 key 去重、同名同地址餐廳避免重複建立。
- 完成：.NET Worker 定期匯入、啟動時匯入、匯入成功/失敗紀錄查詢。
- 部分完成：評論功能目前只有單一整體分數、簡短留言與評論者名稱，尚未包含味道/服務/環境/CP/再訪五項分數、用餐日期、人均消費、審核狀態與會員限制。
- 部分完成：排行榜目前使用簡易平均分，尚未實作 Bayesian Average、評論者權重、評論品質權重、時間衰減、收藏數、照片品質與近期熱度。
- 未完成：Next.js 前台、會員登入、收藏、照片上傳、地區/分類/價位/距離篩選、Typesense 搜尋、Redis 快取、後台審核、檢舉、反作弊、商家認領、SEO 頁面。
- 建置驗證：`dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1`；`dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1`。

## 2026-05-15：責任邊界整理與 migration 基礎

- 完成：完成餐廳/評論/排行責任邊界整理，將 Restaurant、Review、Ranking、Import Target 的 service 與 repository 拆分。
- 完成：導入輕量資料庫 migration 基礎，新增 `schema_migrations` 與版本化 `DatabaseMigrations`，後續 schema 變更可用 migration 追蹤。
- 部分完成：餐廳資料欄位已擴充 BranchName、City、District、Latitude、Longitude、OpeningHours、PriceMin、PriceMax、CuisineType、Tags、Description、OfficialUrl、GoogleMapUrl。尚未建立獨立城市/行政區/料理分類資料表。
- 部分完成：餐廳搜尋已支援 keyword、status、city、district、cuisineType、priceMin、priceMax、minScore、page、pageSize。尚未支援距離搜尋、附近餐廳、Typesense typo tolerance。
- 部分完成：餐廳排序已支援 latest、ranking、reviewCount、favoriteCount；排行榜與列表排序已使用 approved / non-suspicious / non-deleted reviews 的 Bayesian 平台分數，並納入收藏數 FavoriteScore 訊號。尚未納入照片品質、近期熱度與使用者權重。
- 部分完成：評論模型已支援 TasteScore、ServiceScore、EnvironmentScore、ValueScore、RevisitScore、Content、VisitDate、PricePerPerson、DiningType、CompanionType、狀態欄位、會員歸屬與 30 天重複評論限制，並已支援匿名評論檢舉。尚未實作評論照片與會員權重。
- 完成：後台評論審核 API 已支援評論列表查詢、狀態更新、批次狀態更新、可疑標記、刪除標記、審核原因紀錄、操作紀錄查詢、審核紀錄全域搜尋、檢舉列表查詢與檢舉處理，且已加入 Admin role JWT 授權保護。
- 部分完成：自動反作弊 MVP 已支援同評論者短時間大量評論、同餐廳重複內容、低品質內容、與既有餐廳平均分差距過大的規則式偵測；命中後自動標記 `is_suspicious`、狀態設為 Suspicious，並保存 `suspicious_reason` 與 `suspicious_detected_at`。尚未支援 IP/裝置指紋、會員信任分數與批次重算。
- 完成：會員評論歸屬與 30 天限制已落地，登入會員新增完整評論時會從 User JWT 寫入 `restaurant_reviews.user_id`，且同會員同餐廳 30 天內不可重複新增有效評論。
- 部分完成：會員系統基礎已支援 users 資料表、一般會員註冊/登入、PBKDF2 密碼雜湊與 User JWT。尚未實作會員資料維護、Refresh Token 與 email 驗證。
- 完成：後台登入/授權已支援第一位管理員 bootstrap、PBKDF2 密碼雜湊、JWT 登入、Admin Refresh Token 輪替與撤銷、Admin API 授權限制、管理員列表、啟用/停用管理員、目前登入管理員改密碼、角色更新與 ReviewModerator 權限細分，且審核操作可關聯管理員。
- 部分完成：Next.js 前台 `FoodTrust.Web` 已建立，支援首頁、餐廳列表、餐廳詳細、登入、註冊、我的收藏、收藏操作與新增評論的第一版流程。
- 仍未完成：照片上傳、地圖/距離搜尋、Typesense、Redis、商家認領、SEO 頁面。
- 建置驗證：`dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1`；`dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1`。

## 2026-05-18：後台補強、收藏落地、前台骨架

- 完成：登入會員新增評論會套用 User role JWT 授權，從 `ClaimTypes.NameIdentifier` 寫入 `restaurant_reviews.user_id`。
- 完成：新增同會員同餐廳 30 天內不可重複新增評論的服務層檢查，避免重複評論進入評分資料。
- 完成：JWT 驗證設定改為同時接受 AdminJwt 與 UserJwt issuer/audience，避免會員 token 在受保護 API 被拒。
- 完成：收藏功能基礎已落地，新增 `favorite_restaurants` 資料表，支援會員收藏/取消收藏餐廳與查詢我的收藏餐廳分頁列表。
- 完成：後台管理員列表與啟用/停用管理已落地，新增 `GET /api/v1/admin/users` 與 `PATCH /api/v1/admin/users/{id}/active`，並避免管理員停用自己。
- 完成：目前登入管理員改密碼已落地，新增 `PATCH /api/v1/admin/users/me/password`，會驗證目前密碼、新密碼長度與帳號啟用狀態。
- 完成：後台評論批次審核已落地，新增 `PATCH /api/v1/admin/reviews/status`，可一次更新多筆評論狀態並為每筆建立審核紀錄，回傳成功筆數與找不到的評論 ID。
- 完成：後台審核紀錄搜尋已落地，新增 `GET /api/v1/admin/reviews/moderation-logs`，支援 reviewId、adminUserId、action、from、to、page、pageSize 篩選。
- 完成：Admin Refresh Token 已落地，登入會簽發 refresh token，新增 `POST /api/v1/admin/auth/refresh` 輪替 access/refresh token，以及 `POST /api/v1/admin/auth/revoke` 撤銷 refresh token。
- 完成：後台權限細分已落地，新增 ReviewModerator 角色與 `Admin.ReviewModeration` policy，評論審核/檢舉 API 開放 Admin 或 ReviewModerator 使用，管理員管理仍限 Admin，並新增 `PATCH /api/v1/admin/users/{id}/role` 更新角色。
- 完成：排行收藏訊號整合已落地，餐廳列表與排行榜回傳 `favoriteCount`，列表支援 `sortBy=favoriteCount`，排行榜分數納入 FavoriteScore 5% 權重。
- 完成：`FoodTrust.Web` 前台骨架已建立，使用 TypeScript + Next.js App Router + Tailwind CSS，已完成首頁、餐廳搜尋列表、餐廳詳細、會員登入/註冊、我的收藏、收藏按鈕與評論表單第一版。
- 完成：`FoodTrust.Web` 前端互動改善，新增會員狀態列與登出、餐廳列表分頁控制，並在評論送出成功後刷新餐廳詳細頁資料。
- 完成：`FoodTrust.Web` 收藏狀態同步，收藏列表會同步本機會員收藏狀態，餐廳詳情頁收藏按鈕會依狀態初始化並在切換後刷新資料。
- 完成：`FoodTrust.Web` API 錯誤處理補強，前端會解析後端錯誤 payload，登入、註冊、收藏、評論與收藏列表可顯示更精準的失敗訊息。
- 完成：`FoodTrust.Web` SEO route 與視覺細節補強，新增頁面 metadata、餐廳詳情動態 metadata、`robots.txt`、`sitemap.xml`、焦點樣式與餐廳外部連結。
- 完成：`FoodTrust.Web` 部署設定文件補強，新增 `.env.example`、更新 README，記錄 `NEXT_PUBLIC_API_BASE_URL`、`NEXT_PUBLIC_SITE_URL` 與建置部署指令。
- 完成：`FoodTrust.Web` 前端驗證流程補強，新增 typecheck 與 verify npm script，並建立 GitHub Actions workflow 自動執行 lint、typecheck、build。
- 完成：AWS Amplify Hosting 部署準備補強，新增 monorepo `amplify.yml`，並於 `FoodTrust.Web` README 記錄 App root、build artifact、cache 與環境變數設定。
- 進度小結：後台管理補強已完成至權限細分，排行已整合收藏訊號，`FoodTrust.Web` 前台 MVP 骨架已建立；本版準備推送 main 至 origin/main。
- 建置驗證：`dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1`；`dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1`。

## 2026-05-19：AWS Amplify 部署嘗試

- 完成：本機 `FoodTrust.Web` 前端已成功啟動（`http://127.0.0.1:3000`），新增 `.env.local` 設定本機 API base URL。若後端 API 未啟動，首頁/列表/詳細頁可能因 API 無回應載入失敗，登入頁可正常開啟。
- 完成：為處理 Amplify 預設網域 404，將 Next.js 從 16.2.6 調整為 Amplify SSR 支援範圍內的 15.5.18，並同步修正 eslint-config-next 與 ESLint flat config，通過本機 lint、typecheck 與 next build。
- 完成：已提交並推送 commit「修正 Amplify 前端部署 404」到 origin/main。
- AWS 狀態：Amplify App ID `d2p8rcvp0wb9cp`，區域 ap-southeast-2（雪梨），預設網域 `https://main.d2p8rcvp0wb9cp.amplifyapp.com`。CloudFront/HTTP 有回應，但 HTTPS 網頁顯示 404。
- AWS 狀態：CloudShell 執行 `update-app` 後 App platform 已顯示為 `WEB_COMPUTE`（切到 Next.js SSR/Compute 類型）。
- 阻塞：部署 build 階段成功，但部署階段失敗，錯誤為「Failed to find the deploy-manifest.json file in the build output」，判斷偏向 monorepo 設定未讓 Amplify SSR 正確辨識 `FoodTrust.Web` 為 App root。
- 待驗證的修法：設定環境變數 `AMPLIFY_MONOREPO_APP_ROOT=FoodTrust.Web` 並重新觸發 RELEASE job；若仍失敗需檢查 SSR rewrite 設定，必要時重建 Amplify App 並在建立時明確指定 monorepo App root。

（此 Amplify 路線後續已於 2026-06-10 決定停用，改走 Vercel + Cloud Run，見下方。）

## 2026-06-10：AWS 退場決策 + 新雲端部署決策

前端不再繼續放 AWS，改採前端 Vercel、API/Worker Google Cloud Run、資料庫 TiDB Cloud Starter；AWS 資源準備退場。完整決策理由、考慮過的替代方案（Supabase、Cloud SQL/Azure MySQL）與後果見 [adr/0001-cloud-deployment-vercel-cloud-run-tidb.md](adr/0001-cloud-deployment-vercel-cloud-run-tidb.md)。AWS 退場清單見 [ops/aws-teardown.md](ops/aws-teardown.md)。

## 2026-06-12：TiDB / GCP 部署接續

- 完成：TiDB Cloud Starter 已建立 `FoodTrust-DEV` 實例，FDA 候選資料匯入流程已可寫入 `candidate_restaurants`。
- 完成：匯入流程改為「先進候選表、人工審核後再寫入 restaurants」，並新增候選餐廳 API、後台頁面 `/admin/candidate-restaurants`、Approve/Reject 流程與 Google Search 按鈕。
- 完成：`FoodTrust.Worker` 已補 `launchSettings.json`、`appsettings*.json` 複製規則、TiDB CA / TLS / BootstrapDatabase 設定欄位，且已加回 solution。
- 完成：`FoodTrust.Web` API client 已改為必須明確設定 `NEXT_PUBLIC_API_BASE_URL`，不再 fallback 到本機 `localhost:5000`。
- 完成：新增 `FoodTrust.Api/Dockerfile`、`.dockerignore`、`.gcloudignore`、`cloudbuild.api.yaml`，可使用 Cloud Build 建置 API image。
- 完成：GCP 專案 `foodtrust-dev`、Artifact Registry `foodtrust`、必要 API（Cloud Run Admin、Artifact Registry、Secret Manager、Cloud Scheduler、Cloud Build）均已建立或啟用。
- 完成：Cloud Build 已成功將 API image 推送到 `asia-east1-docker.pkg.dev/foodtrust-dev/foodtrust/foodtrust-api:dev`。
- 完成：API 已補 CORS policy，預設允許 `http://localhost:3000` 與 `http://127.0.0.1:3000`。
- 阻塞：Cloud Run `foodtrust-api` 首次 deploy 失敗，錯誤表面為容器未在 `PORT=8080` 上啟動；高機率根因是啟動時先執行 `DatabaseInitializer.InitializeAsync()`，但 Cloud Run 尚未提供正確的 TiDB connection string / JWT / CORS 正式設定，導致容器在 listen 前 crash。此阻塞截至最新一次更新（見 [handoff.md](handoff.md)）仍未解除。

## 2026-07-21：RESTful + Clean Architecture + DDD 整理

將專案整理為 RESTful API + Clean Architecture + DDD 實作：路由改為資源導向（如 `/sessions`、`/favorites`）、Core 新增多個 domain aggregate 與 value object、業務規則從 controller/repository 的 primitive validation 收斂到 domain model。完整路由對照表、分層說明與 DDD 清單見 [adr/0002-restful-clean-architecture-ddd.md](adr/0002-restful-clean-architecture-ddd.md)。驗證：`dotnet build` FoodTrust.Api / FoodTrust.Worker、`npm run build`（FoodTrust.Web）均通過；API build 仍有 `Microsoft.OpenApi 2.3.0` high severity 套件警告待升級。

## 2026-07-23：文件整理

- 完成：將 `FoodTrust_project_spec.md` 拆分——規格本體留在原檔並修正多處 DOCX 匯出遺留的格式問題（表格破損、API 範例斷行遺失、文字黏在一起）；開發進度/部署阻塞/下一步移到 [handoff.md](handoff.md)（覆寫式現況快照）；歷史紀錄整理進本檔（append-only）；AWS 帳單/退場清單移到 [ops/aws-teardown.md](ops/aws-teardown.md)。

## 2026-07-24：文件規範補強（docs/README、ADR、修正過期/矛盾內容）

- 完成：修正 `FoodTrust_project_spec.md` 開頭與 DOCX 相關的過期敘述——原文說 DOCX 作為「閱讀或交付版本」，但 Word 備份已決定不進 repo（見 [archive/README.md](archive/README.md)），已改為指向唯一維護版本 Markdown 的說明。
- 完成：`handoff.md` 的「最後更新」日期過去停留在 2026-07-21，即使文件整理已經做到 2026-07-23 也沒同步更新，容易讓人誤判文件是否過期；已改為每次異動都同步更新該日期。
- 完成：新增 [README.md](README.md) 作為 `docs/` 入口，列出各文件用途、更新方式，以及「什麼不該放進 repo」（Word/PDF 備份、手動抄寫的 commit 清單、debug 過程貼文）。
- 完成：`FoodTrust_project_spec.md` 12.1 節「建議方案結構」明確標記為早期建議、非目前實作，新增 12.2 節列出目前實際採用的 Clean Architecture 分層（Core/Infrastructure/Api/Worker/Web），避免規格書內部前後矛盾。
- 完成：新增 [adr/](adr/) 目錄，把原本寫在 devlog 裡的兩個重要架構決策拆成獨立 ADR——[adr/0001-cloud-deployment-vercel-cloud-run-tidb.md](adr/0001-cloud-deployment-vercel-cloud-run-tidb.md)（2026-06-10 雲端部署決策）與 [adr/0002-restful-clean-architecture-ddd.md](adr/0002-restful-clean-architecture-ddd.md)（2026-07-21 RESTful/Clean Architecture/DDD 整理），devlog 對應段落改為摘要 + 連結。
