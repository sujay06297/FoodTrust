# FoodTrust / 食信 專案規格書

> 整理版：由既有 DOCX 規格書匯出後整理，此 Markdown 為唯一維護版本。原始 DOCX 備份不進版本控管，原因與已移除清單見 [docs/archive/README.md](docs/archive/README.md)。
>
> 本檔案只保留產品規格本體。文件入口（含各文件用途說明）見 [docs/README.md](docs/README.md)。開發進度、部署狀態與下一步待辦請見 [docs/handoff.md](docs/handoff.md)；歷史演進紀錄請見 [docs/devlog.md](docs/devlog.md)；重要架構決策見 [docs/adr/](docs/adr/)。

FoodTrust / 食信 — 類 Tabelog 美食排行網專案規格書

| 項目 | 內容 |
| --- | --- |
| 文件版本 | v1.0 |
| 文件日期 | 2026-05-15 |
| 定位 | 台灣可信賴的美食排行與餐廳評價平台 |

## 文件資訊

| 項目 | 內容 |
| --- | --- |
| 專案名稱 | FoodTrust / 食信 |
| 專案類型 | 美食排行、餐廳評論、餐廳搜尋平台 |
| 目標市場 | 台灣餐廳與美食搜尋市場 |
| 核心定位 | 可信度加權排行，不只做平均星等 |
| 技術建議 | Next.js + ASP.NET Core Web API + MySQL/PostgreSQL + Redis + Typesense + .NET Worker |
| MVP 目標 | 餐廳搜尋、排行、詳細頁、評論、收藏、後台審核 |

## 目錄

- 1. 專案概述
- 2. 專案背景與市場痛點
- 3. 專案目標
- 4. 目標使用者與角色權限
- 5. MVP 範圍
- 6. 功能需求規格
- 7. 排行與評分模型
- 8. 反作弊與內容治理
- 9. 資料來源規劃
- 10. 頁面規格
- 11. 資料表設計
- 12. 技術架構建議
- 13. API 規格範例
- 14. 後台管理規格
- 15. SEO 規劃
- 16. 開發階段規劃
- 17. 風險與解法
- 18. 成功指標
- 19. MVP 優先順序
- 20. 結論與建議

## 1. 專案概述

本專案旨在建立一個類似日本 Tabelog 的台灣美食排行與餐廳評價平台。平台核心價值不是單純蒐集餐廳資料或提供五星評價，而是透過加權評分、評論品質判斷與反作弊機制，建立更可信的餐廳排行榜。

第一版建議以 MVP 可落地為主，不先投入訂位、外送、商家 CRM 或完整 App，而是先完成餐廳資料庫、搜尋、排行、評論、收藏與後台審核。

| 項目 | 說明 |
| --- | --- |
| 產品定位 | 台灣可信賴的美食排行與餐廳評價平台 |
| 主要價值 | 排行可信、分類清楚、評論品質高、避免灌水 |
| 第一版策略 | 先做台北、台中、高雄重點餐廳與熱門分類 |
| 產品口號 | 不是最多評論，而是更可信的美食排行 |

## 2. 專案背景與市場痛點

台灣使用者目前找餐廳主要依賴 Google Maps、Instagram、Threads、部落格、米其林指南與親友推薦。這些來源各有優點，但在餐廳排行、評論可信度、分類精細度與搜尋體驗上仍有空間。

| 平台 / 來源 | 優點 | 主要問題 |
| --- | --- | --- |
| Google Maps | 資料量大、地圖方便 | 星等容易灌水，分類較粗，排行不完全以美食品質為主 |
| Instagram / Threads | 圖片吸引人，流量強 | 搜尋與結構化排行弱，偏曝光不偏品質 |
| 部落格 | 內容詳細 | 更新慢，資料分散，難以形成即時排行 |
| 米其林指南 | 權威性高 | 覆蓋餐廳有限，偏精選名單 |
| 親友推薦 | 信任度高 | 覆蓋範圍小，難以搜尋與比較 |

### 2.1 核心機會

- 台灣缺少一個以美食排行為核心、且有可信度機制的平台。
- Google Maps 很強，但不是專門為美食排名設計。
- 社群平台有聲量，但缺少結構化搜尋與長期累積。
- 若能把「餐廳資料庫 + 搜尋 + 加權排行 + 高品質評論」做好，有機會形成差異化。

## 3. 專案目標

### 3.1 核心目標

- 讓使用者可以依地區、料理類型、價位、評分、評論可信度快速找到餐廳。
- 建立餐廳排行與評論系統，避免單純平均星等造成誤導。
- 建立後台審核與檢舉機制，降低假評論與錯誤資料。
- 透過 SEO 與榜單內容取得自然流量。

### 3.2 差異化目標

- 不主打最多評論，而是主打有效評論與可信排行。
- 評論者權重、評論品質權重與時間衰減納入評分。
- 排行與廣告必須分離，避免商業化傷害信任。

## 4. 目標使用者與角色權限

| 角色 | 主要需求 | 可使用功能 |
| --- | --- | --- |
| 訪客 | 找餐廳、看排行、看評論 | 瀏覽餐廳、搜尋、查看評論、查看照片 |
| 一般會員 | 發表心得、收藏餐廳、建立清單 | 評論、上傳照片、收藏、檢舉、建立個人清單 |
| 認證評論者 | 累積美食影響力 | 較高評論權重、評論優先顯示、專屬標章 |
| 餐廳業者 | 管理店家資訊、回覆評論 | 認領餐廳、編輯資料、上傳菜單、回覆評論 |
| 管理員 | 維護平台品質 | 餐廳審核、評論審核、檢舉處理、黑名單、排行參數管理 |

## 5. MVP 範圍

### 5.1 第一版必做

| 模組 | 功能 |
| --- | --- |
| 前台 | 餐廳列表、餐廳詳細頁、搜尋、篩選、排行榜、評論列表、發表評論、照片、收藏、會員登入 |
| 後台 | 餐廳資料管理、評論審核、照片審核、分類管理、地區管理、使用者管理、檢舉處理 |
| 背景工作 | 餐廳分數計算、搜尋索引同步、可疑評論標記 |
| 資料 | 城市、行政區、料理分類、餐廳資料、評論資料、收藏資料 |

### 5.2 第一版暫不做

- 線上訂位
- 外送串接
- POS 串接
- 付費廣告
- 完整商家 CRM
- AI 推薦
- 社群動態牆
- 手機 App

## 6. 功能需求規格

### 6.1 餐廳資料

| 欄位 | 說明 |
| --- | --- |
| RestaurantId | 餐廳 ID |
| Name | 餐廳名稱 |
| BranchName | 分店名稱 |
| City / District | 縣市與行政區 |
| Address | 地址 |
| Latitude / Longitude | 經緯度 |
| Phone | 電話 |
| OpeningHours | 營業時間 |
| PriceMin / PriceMax | 價格區間 |
| CuisineType | 料理類型 |
| Tags | 標籤 |
| Description | 餐廳介紹 |
| OfficialUrl | 官方網站 |
| GoogleMapUrl | Google Map 連結 |
| Status | 營業狀態 |

### 6.2 餐廳搜尋與排序

| 搜尋條件 | 說明 |
| --- | --- |
| 關鍵字 | 餐廳名稱、分店名、標籤、料理類型 |
| 地區 | 縣市、行政區、附近餐廳 |
| 類型 | 拉麵、燒肉、火鍋、咖啡廳、小吃等 |
| 價位 | 人均消費區間 |
| 評分 | 平台分數範圍 |
| 營業狀態 | 營業中、已歇業、暫停營業 |
| 排序 | 綜合排行、評分最高、評論最多、最新評論、收藏最多、距離最近 |

### 6.3 評論功能

| 欄位 | 說明 |
| --- | --- |
| TasteScore | 味道分數 |
| ServiceScore | 服務分數 |
| EnvironmentScore | 環境分數 |
| ValueScore | CP 值分數 |
| RevisitScore | 再訪意願 |
| Content | 評論內容 |
| VisitDate | 用餐日期 |
| PricePerPerson | 人均消費 |
| DiningType | 早午餐、午餐、晚餐、宵夜 |
| CompanionType | 一人、約會、朋友、家庭、商務 |
| Status | 審核狀態 |

### 6.4 評論限制

- 評論字數至少 30 字。
- 評分必須搭配文字內容。
- 同一使用者同一餐廳 30 天內只計入一次分數。
- 新帳號評論權重較低。
- 被大量檢舉的帳號降低權重。
- 可疑評論不進入排行計算。
- 業者帳號不可評論自己的餐廳。

## 7. 排行與評分模型

本專案不建議使用單純平均分。單純平均分容易讓少量五星評論的小店超越大量穩定好評的餐廳，也容易被灌水。第一版建議使用簡化版 Bayesian Average。

```text
平台分數 = (v / (v + m)) × R + (m / (v + m)) × C
```

| 參數 | 說明 |
| --- | --- |
| R | 該餐廳有效評論平均分 |
| v | 該餐廳有效評論數 |
| m | 最低可信評論門檻，例如 20 |
| C | 全站平均分，例如 3.6 |

### 7.1 評論者權重

```text
有效評論分數 = 原始分數 × 使用者權重 × 評論品質權重 × 時間衰減權重
```

| 條件 | 建議權重 |
| --- | --- |
| 新帳號 | 0.3 |
| 一般會員 | 1.0 |
| 認證評論者 | 1.2 |
| 高可信評論者 | 1.5 |
| 可疑帳號 | 0 或不計入 |

### 7.2 評論品質權重

| 條件 | 建議權重 |
| --- | --- |
| 只有短評 | 0.5 |
| 有詳細文字 | 1.0 |
| 有照片 | 1.1 |
| 有菜色、人均、用餐情境 | 1.2 |
| 被多數使用者認為有幫助 | 1.3 |

### 7.3 時間衰減

| 評論時間 | 建議權重 |
| --- | --- |
| 最近 6 個月 | 100% |
| 6～12 個月 | 80% |
| 1～2 年 | 60% |
| 2 年以上 | 40% |

### 7.4 排行榜分數

```text
RankingScore = PlatformScore × 0.65
             + ReviewQualityScore × 0.15
             + RecentPopularityScore × 0.10
             + FavoriteScore × 0.05
             + PhotoQualityScore × 0.05
```

| 指標 | 權重 |
| --- | --- |
| 平台分數 | 65% |
| 評論品質 | 15% |
| 近期熱度 | 10% |
| 收藏數 | 5% |
| 照片品質 | 5% |

## 8. 反作弊與內容治理

### 8.1 可疑評論判斷條件

- 同 IP 短時間大量評論
- 同裝置大量帳號
- 新帳號只評論同一家店
- 評論內容高度相似
- 短時間大量五星或一星
- 業者登入位置與評論帳號高度重疊
- 評論時間集中在非營業時間
- 評論文字像模板

### 8.2 評論狀態

| 狀態 | 說明 |
| --- | --- |
| Pending | 待審核 |
| Approved | 已通過 |
| Rejected | 已拒絕 |
| Hidden | 隱藏 |
| Suspicious | 可疑 |
| Deleted | 已刪除 |

### 8.3 分數計算條件

```text
Status = Approved
AND IsSuspicious = false
AND IsDeleted = false
```

## 9. 資料來源規劃

| 階段 | 作法 | 目標 |
| --- | --- | --- |
| 第一階段 | 人工建檔 | 台北、台中、高雄熱門餐廳各 300～500 間 |
| 第二階段 | 使用者補資料 | 允許會員新增餐廳，進後台審核 |
| 第三階段 | 商家認領 | 業者維護資料，但重要變更仍需審核 |
| 第四階段 | 外部資料輔助 | 參考公開榜單、米其林、社群熱門店，但不直接作為平台分數 |

### 9.1 初期優先分類

- 拉麵
- 燒肉
- 火鍋
- 咖啡廳
- 甜點
- 牛肉麵
- 居酒屋
- 台菜
- 小吃
- 早午餐

## 10. 頁面規格

| 頁面 | 主要內容 |
| --- | --- |
| 首頁 | 搜尋框、熱門地區、熱門料理分類、今日熱門餐廳、最新高分評論、編輯精選榜單 |
| 餐廳列表頁 | 關鍵字搜尋、篩選、排序、分頁、餐廳卡片 |
| 餐廳詳細頁 | 基本資訊、分數統計、照片牆、推薦菜色、評論列表、地圖、相似餐廳 |
| 評論撰寫頁 | 五項分數、評論內容、用餐日期、人均價格、用餐情境、照片上傳 |
| 使用者個人頁 | 暱稱、頭像、評論數、收藏數、可信度等級、評論列表、收藏餐廳 |
| 後台管理頁 | 餐廳管理、評論管理、照片管理、分類管理、使用者管理、檢舉管理 |

## 11. 資料表設計

### 11.1 Restaurant

```sql
CREATE TABLE Restaurant (
    RestaurantId BIGINT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    BranchName VARCHAR(100) NULL,
    CityId INT NOT NULL,
    DistrictId INT NOT NULL,
    Address VARCHAR(255) NOT NULL,
    Latitude DECIMAL(10, 7) NULL,
    Longitude DECIMAL(10, 7) NULL,
    Phone VARCHAR(30) NULL,
    PriceMin INT NULL,
    PriceMax INT NULL,
    Description TEXT NULL,
    OfficialUrl VARCHAR(500) NULL,
    GoogleMapUrl VARCHAR(500) NULL,
    Status TINYINT NOT NULL,
    CreatedTime DATETIME NOT NULL,
    UpdatedTime DATETIME NOT NULL
);
```

### 11.2 RestaurantReview

```sql
CREATE TABLE RestaurantReview (
    ReviewId BIGINT PRIMARY KEY AUTO_INCREMENT,
    RestaurantId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    TasteScore DECIMAL(3,2) NOT NULL,
    ServiceScore DECIMAL(3,2) NOT NULL,
    EnvironmentScore DECIMAL(3,2) NOT NULL,
    ValueScore DECIMAL(3,2) NOT NULL,
    RevisitScore DECIMAL(3,2) NOT NULL,
    AverageScore DECIMAL(3,2) NOT NULL,
    Content TEXT NOT NULL,
    VisitDate DATE NULL,
    PricePerPerson INT NULL,
    DiningType TINYINT NULL,
    CompanionType TINYINT NULL,
    Status TINYINT NOT NULL,
    IsSuspicious BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedTime DATETIME NOT NULL,
    UpdatedTime DATETIME NOT NULL
);
```

### 11.3 RestaurantScore

```sql
CREATE TABLE RestaurantScore (
    RestaurantId BIGINT PRIMARY KEY,
    RawAverageScore DECIMAL(4,2) NOT NULL,
    PlatformScore DECIMAL(4,2) NOT NULL,
    RankingScore DECIMAL(8,4) NOT NULL,
    EffectiveReviewCount INT NOT NULL,
    TotalReviewCount INT NOT NULL,
    FavoriteCount INT NOT NULL,
    PhotoCount INT NOT NULL,
    LastReviewTime DATETIME NULL,
    UpdatedTime DATETIME NOT NULL
);
```

### 11.4 其他資料表

- RestaurantPhoto：餐廳照片、菜單照片、使用者上傳照片。
- FavoriteRestaurant：會員收藏餐廳。
- RestaurantCategory：料理分類。
- RestaurantCategoryMapping：餐廳與分類對應。
- RestaurantReport：檢舉資料。
- RestaurantClaimRequest：商家認領申請。

## 12. 技術架構建議

| 層級 | 建議技術 | 說明 |
| --- | --- | --- |
| 前端 | Next.js + React + Tailwind CSS | SEO 友善，適合餐廳內容頁 |
| 後端 | ASP.NET Core Web API | 符合既有 C#/.NET 技術背景 |
| 資料庫 | MySQL 或 PostgreSQL | 儲存主資料、評論、會員、後台資料 |
| 搜尋 | Typesense | 適合 MVP，設定簡單，支援 typo tolerance |
| 快取 | Redis | 熱門排行、餐廳分數、收藏狀態、Rate Limit |
| 背景工作 | .NET Worker | 分數計算、索引同步、可疑評論分析 |
| 物件儲存 | S3 或相容服務 | 餐廳與評論照片 |

> 目前實際採用的雲端部署（前端 Vercel、API/Worker Google Cloud Run、資料庫 TiDB Cloud Starter）與此表的初期建議不完全相同；決策過程與理由見 [docs/devlog.md](docs/devlog.md) 2026-06-10 項目，目前狀態見 [docs/handoff.md](docs/handoff.md)。

### 12.1 建議方案結構（早期建議，非目前實作）

下方是規劃初期（2026-05-15）的建議結構，僅作為設計意圖參考，**與目前實作已不一致**：

```text
FoodTrust.Web       // Next.js 前台
FoodTrust.Api       // ASP.NET Core API
FoodTrust.Admin     // 後台管理系統
FoodTrust.Worker    // 背景工作
FoodTrust.Data      // Repository / DataService
FoodTrust.Search    // Typesense 同步與查詢
FoodTrust.Common    // 共用模型、Enum、Util
```

### 12.2 目前實作架構（現況）

實際採用 Clean Architecture 分層，取代上方 12.1 的建議：`FoodTrust.Core`（domain/application，內層）、`FoodTrust.Infrastructure`（Dapper/MySQL、migration、外部匯入，外層）、`FoodTrust.Api`（controller/JWT/DI，composition root）、`FoodTrust.Worker`（背景匯入）、`FoodTrust.Web`（Next.js 前台）。並無獨立的 `FoodTrust.Admin`、`FoodTrust.Data`、`FoodTrust.Search`、`FoodTrust.Common` 專案；後台管理是 `FoodTrust.Api` 內的 Admin 路由與角色權限，Typesense/Redis 尚未導入。完整現況見 [docs/handoff.md](docs/handoff.md)，架構決策脈絡見 [docs/adr/](docs/adr/)。

## 13. API 規格範例

### 13.1 搜尋餐廳

```http
GET /api/restaurants/search?keyword=拉麵&cityId=1&districtId=5&categoryId=10&minScore=3.5&priceMin=100&priceMax=500&sortBy=ranking&page=1&pageSize=20
```

### 13.2 取得餐廳詳細資料

```http
GET /api/restaurants/{restaurantId}
```

### 13.3 新增評論

```http
POST /api/restaurants/{restaurantId}/reviews
Content-Type: application/json

{
  "tasteScore": 4.5,
  "serviceScore": 4.0,
  "environmentScore": 3.8,
  "valueScore": 4.2,
  "revisitScore": 4.5,
  "content": "餐點味道穩定，湯頭濃度夠，價格合理，尖峰時間需要排隊。",
  "visitDate": "2026-05-15",
  "pricePerPerson": 350,
  "diningType": 2,
  "companionType": 3
}
```

> 實際上線的路由已依 RESTful 慣例調整（例如會員/管理員登入改為對 `sessions` 資源 POST），詳見 [docs/devlog.md](docs/devlog.md) 2026-07-21 項目。此節保留原始規格範例作為設計意圖參考。

## 14. 後台管理規格

| 模組 | 功能 |
| --- | --- |
| 餐廳審核 | 查看待審核餐廳、修改資料、通過、拒絕、合併重複餐廳 |
| 評論審核 | 查看新評論、查看可疑評論、通過、隱藏、刪除、封鎖使用者 |
| 檢舉管理 | 處理假評論、錯誤資訊、攻擊內容、廣告、歇業回報 |
| 排行參數管理 | 調整平台分數、評論品質、近期熱度、收藏與照片權重 |
| 分類管理 | 新增、修改、排序、停用料理分類與標籤 |
| 使用者管理 | 查看會員狀態、評論紀錄、檢舉紀錄、可信度等級 |

## 15. SEO 規劃

每個餐廳頁、城市排行榜、料理分類排行榜都應該有獨立 URL，讓 Google 可以收錄。

餐廳頁範例：

```text
URL: /{city}/{district}/restaurants/{restaurantId}
範例: /taipei/daan/restaurants/12345
Title: 店名｜台北大安區拉麵推薦｜FoodTrust 食信
Meta Description: 查看店名的真實用餐評價、價格、營業時間、推薦菜色與台北大安區拉麵排行榜。
```

## 16. 開發階段規劃

| 階段 | 期間 | 功能 | 目標 |
| --- | --- | --- | --- |
| Phase 1：MVP | 2～3 個月 | 餐廳資料、搜尋、詳細頁、評論、收藏、基本排行榜、後台 | 完成可上線版本 |
| Phase 2：可信度強化 | 1～2 個月 | 評論者權重、可疑評論偵測、評論品質分、商家認領 | 提高排行可信度 |
| Phase 3：內容與流量 | 2～3 個月 | 城市排行、料理排行、主題榜單、SEO 文章、社群分享圖 | 累積自然流量 |
| Phase 4：商業化 | 視流量而定 | 訂位、商家方案、精選曝光、數據報表 | 建立收入模型 |

## 17. 風險與解法

| 風險 | 問題 | 解法 |
| --- | --- | --- |
| 初期資料量不足 | 沒有評論就沒有排行 | 先做編輯精選榜，人工建立 500～1000 間餐廳，開放使用者補資料 |
| 評論可信度不足 | 容易變成灌水平台 | 不採單純平均分，建立評論者權重與可疑評論偵測 |
| Google Maps 已經很強 | 使用者習慣直接用 Google Maps | 不要比地圖，要比美食分類、排行可信度與主題榜單 |
| 商業化傷害信任 | 餐廳買排名會破壞平台價值 | 廣告與自然排行分開，付費曝光明確標示 |
| 維護成本過高 | 餐廳資訊常變動 | 開放使用者回報與商家認領，但重要變更需審核 |

## 18. 成功指標

| 類型 | 指標 | MVP 目標 |
| --- | --- | --- |
| 產品 | 餐廳數 | 500+ |
| 產品 | 有效評論數 | 2,000+ |
| 產品 | 月活使用者 | 10,000+ |
| 產品 | 搜尋轉換率 | 搜尋後點進餐廳頁 |
| 產品 | 收藏率 | 餐廳頁收藏比例 |
| 品質 | 可疑評論比例 | 低於 10% |
| 品質 | 被檢舉評論處理時間 | 48 小時內 |
| 品質 | 餐廳資料錯誤率 | 持續下降 |

## 19. MVP 優先順序

| 優先級 | 功能 |
| --- | --- |
| 第一優先 | 餐廳資料表、餐廳搜尋、餐廳詳細頁、評論功能、分數計算、排行榜、後台審核 |
| 第二優先 | 收藏、照片、使用者等級、可疑評論偵測、Typesense 搜尋 |
| 第三優先 | 商家認領、主題榜單、SEO 文章、地圖模式、訂位功能 |

## 20. 結論與建議

此專案可行，但成功關鍵不是功能數量，而是平台信任感。若只做餐廳列表、五星評論與照片，很容易成為弱版 Google Maps。

較合理的產品方向是：餐廳資料庫 + 高品質評論 + 加權評分模型 + 反作弊機制 + 地區 / 類型排行榜 + SEO 流量。

第一版建議先聚焦「找餐廳 → 看排行 → 看評論 → 收藏 → 自己寫評論」這條核心流程，先讓產品有明確價值，再逐步加入商家認領、訂位與商業化功能。

### 附錄 A：參考資料

- Tabelog 官方說明：餐廳分數並非單純平均，而是透過演算法計算。
- 米其林指南台灣名單：可作為餐廳標籤或外部權威參考，但不建議直接作為平台評分來源。

---

目前實作進度、部署阻塞與下一步待辦不再寫在本檔案，請見 [docs/handoff.md](docs/handoff.md)（現況快照）、[docs/devlog.md](docs/devlog.md)（歷史演進紀錄）與 [docs/adr/](docs/adr/)（重要架構決策）。AWS 資源整理/退場清單見 [docs/ops/aws-teardown.md](docs/ops/aws-teardown.md)。文件總覽見 [docs/README.md](docs/README.md)。
