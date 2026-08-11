# FoodTrust / 食信

台灣可信賴的美食排行與餐廳評價平台。核心不是單純的五星平均分，而是加權評分（Bayesian Average + 評論者權重 + 評論品質 + 時間衰減）與反作弊機制，目標是「不是最多評論，而是更可信的美食排行」。

## 技術棧

| 層級 | 技術 | 部署 |
| --- | --- | --- |
| 前台 | Next.js（App Router）+ TypeScript + Tailwind CSS | Vercel |
| API | ASP.NET Core Web API（Clean Architecture） | Google Cloud Run |
| 背景工作 | .NET Worker（資料匯入） | 規劃改 Cloud Run Jobs |
| 資料庫 | MySQL 相容 | TiDB Cloud Starter |
ttt
架構決策脈絡見 [docs/adr/](docs/adr/)。

## 專案結構

```text
FoodTrust.Core            // Domain model、value object、interface、application service（內層）
FoodTrust.Infrastructure  // Dapper/MySQL repository、migration、外部匯入（外層）
FoodTrust.Api             // Controller、JWT、DI 組裝（composition root）
FoodTrust.Worker          // 背景匯入（台灣 FDA 食品業者資料）
FoodTrust.Web             // Next.js 前台
docs/                     // 文件入口見 docs/README.md
```

## 快速開始

### 後端（API / Worker）

需求：.NET 10 SDK、MySQL 相容資料庫（本機開發可用本機 MySQL，正式環境為 TiDB Cloud）。

1. 複製 `FoodTrust.Api/appsettings.json` 的設定區塊到 `FoodTrust.Api/appsettings.Development.json`（此檔已 gitignore，不會被提交），填入本機連線字串與 JWT signing key。
2. 建置（Windows 下請用單執行緒避免 DLL lock）：

   ```powershell
   dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1
   dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1
   ```

3. 執行 API：

   ```powershell
   dotnet run --project FoodTrust.Api\FoodTrust.Api.csproj
   ```

### 前台（Web）

```bash
cd FoodTrust.Web
npm install
npm run dev
```

環境變數與部署細節見 [FoodTrust.Web/README.md](FoodTrust.Web/README.md)。

## 深入文件

- [FoodTrust_project_spec.md](FoodTrust_project_spec.md) — 產品規格本體（功能需求、評分模型、資料表設計、API 範例）
- [docs/README.md](docs/README.md) — 文件索引，說明每份文件的用途與更新方式
- [docs/handoff.md](docs/handoff.md) — 現況快照（架構、已完成/未完成功能、目前部署阻塞、下一步）
- [docs/devlog.md](docs/devlog.md) — 歷史演進紀錄
- [docs/adr/](docs/adr/) — 重要架構決策紀錄
- [docs/ops/](docs/ops/) — 部署與維運操作清單

## 待補

- 照片上傳、地圖/距離搜尋、Typesense 搜尋、Redis 快取、商家認領、SEO 文章頁
- 會員 Refresh Token 與 email 驗證、獨立城市/行政區/料理分類資料表
- 反作弊 IP/裝置指紋與會員信任分數、批次重算
- Cloud Run API 部署目前卡在容器未於 `PORT=8080` 啟動（詳見 [docs/handoff.md](docs/handoff.md)）

完整現況與細節請以 [docs/handoff.md](docs/handoff.md) 為準，本節只列重點。
