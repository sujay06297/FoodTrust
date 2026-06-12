# FoodTrust / 食信 專案規格書

> 整理版：由既有 DOCX 規格書匯出後整理，後續以此 Markdown 作為主要維護檔；DOCX 作為閱讀或交付版本。

FoodTrust / 食信

類 Tabelog 美食排行網專案規格書

文件版本：v1.0文件日期：2026-05-15定位：台灣可信賴的美食排行與餐廳評價平台

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

平台分數 = (v / (v + m)) × R + (m / (v + m)) × C

| 參數 | 說明 |
| --- | --- |
| R | 該餐廳有效評論平均分 |
| v | 該餐廳有效評論數 |
| m | 最低可信評論門檻，例如 20 |
| C | 全站平均分，例如 3.6 |

### 7.1 評論者權重

有效評論分數 = 原始分數 × 使用者權重 × 評論品質權重 × 時間衰減權重

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

RankingScore = PlatformScore × 0.65             + ReviewQualityScore × 0.15             + RecentPopularityScore × 0.10             + FavoriteScore × 0.05             + PhotoQualityScore × 0.05

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

狀態

說明

Pending

待審核

Approved

已通過

Rejected

已拒絕

Hidden

隱藏

Suspicious

可疑

Deleted

已刪除

### 8.3 分數計算條件

Status = ApprovedAND IsSuspicious = falseAND IsDeleted = false

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

### 12.1 建議方案結構

```text
FoodTrust.Web       // Next.js 前台
FoodTrust.Api       // ASP.NET Core API
FoodTrust.Admin     // 後台管理系統
FoodTrust.Worker    // 背景工作
FoodTrust.Data      // Repository / DataService
FoodTrust.Search    // Typesense 同步與查詢
FoodTrust.Common    // 共用模型、Enum、Util
```

## 13. API 規格範例

### 13.1 搜尋餐廳

```http
GET /api/restaurants/searchQuery:keyword=拉麵cityId=1districtId=5categoryId=10minScore=3.5priceMin=100priceMax=500sortBy=rankingpage=1pageSize=20
```

### 13.2 取得餐廳詳細資料

```http
GET /api/restaurants/{restaurantId}
```

### 13.3 新增評論

```http
POST /api/restaurants/{restaurantId}/reviews{  "tasteScore": 4.5,  "serviceScore": 4.0,  "environmentScore": 3.8,  "valueScore": 4.2,  "revisitScore": 4.5,  "content": "餐點味道穩定，湯頭濃度夠，價格合理，尖峰時間需要排隊。",  "visitDate": "2026-05-15",  "pricePerPerson": 350,  "diningType": 2,  "companionType": 3}
```

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

餐廳頁 URL:/{city}/{district}/restaurants/{restaurantId}範例:/taipei/daan/restaurants/12345Title:店名｜台北大安區拉麵推薦｜FoodTrust 食信Meta Description:查看店名的真實用餐評價、價格、營業時間、推薦菜色與台北大安區拉麵排行榜。

## 16. 開發階段規劃

| 階段 | 期間 | 功能 | 目標 |
| --- | --- | --- | --- |
| Phase 1：MVP | 2～3 個月 | 餐廳資料、搜尋、詳細頁、評論、收藏、基本排行榜、後台 | 完成可上線版本 |
| Phase 2：可信度強化 | 1～2 個月 | 評論者權重、可疑評論偵測、評論品質分、商家認領 | 提高排行可信度 |
| Phase 3：內容與流量 | 2～3 個月 | 城市排行、料理排行、主題榜單、SEO 文章、社群分享圖 | 累積自然流量 |
| Phase 4：商業化 | 視流量而定 | 訂位、商家方案、精選曝光、數據報表 | 建立收入模型 |

## 17. 風險與解法

風險

問題

解法

初期資料量不足

沒有評論就沒有排行

先做編輯精選榜，人工建立 500～1000 間餐廳，開放使用者補資料

評論可信度不足

容易變成灌水平台

不採單純平均分，建立評論者權重與可疑評論偵測

Google Maps 已經很強

使用者習慣直接用 Google Maps

不要比地圖，要比美食分類、排行可信度與主題榜單

商業化傷害信任

餐廳買排名會破壞平台價值

廣告與自然排行分開，付費曝光明確標示

維護成本過高

餐廳資訊常變動

開放使用者回報與商家認領，但重要變更需審核

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

附錄 A：參考資料

Tabelog 官方說明：餐廳分數並非單純平均，而是透過演算法計算。

米其林指南台灣名單：可作為餐廳標籤或外部權威參考，但不建議直接作為平台評分來源。

## 實作狀態標記（2026-05-15）

標記說明：已完成 = 已在目前後端專案中落地；部分完成 = 已有雛形但未達規格完整要求；未完成 = 尚未實作。

- 已完成：ASP.NET Core API 專案、Core/Infrastructure 分層、MySQL 初始化、餐廳資料表、餐廳來源表、餐廳匯入紀錄表、餐廳評分表。

- 已完成：餐廳新增、查詢、詳細資料、更新、狀態更新 API。

- 已完成：簡易評分 API（1-5 分）、簡易排行榜 API（依平均分數與評分數排序）。

- 已完成：台灣 FDA 食品業者資料匯入、ZIP/JSON/CSV 解析、餐飲業者篩選、來源 key 去重、同名同地址餐廳避免重複建立。

- 已完成：.NET Worker 定期匯入、啟動時匯入、匯入成功/失敗紀錄查詢。

- 部分完成：評論功能目前只有單一整體分數、簡短留言與評論者名稱，尚未包含味道/服務/環境/CP/再訪五項分數、用餐日期、人均消費、審核狀態與會員限制。

- 部分完成：排行榜目前使用簡易平均分，尚未實作 Bayesian Average、評論者權重、評論品質權重、時間衰減、收藏數、照片品質與近期熱度。

- 未完成：Next.js 前台、會員登入、收藏、照片上傳、地區/分類/價位/距離篩選、Typesense 搜尋、Redis 快取、後台審核、檢舉、反作弊、商家認領、SEO 頁面。

- 目前建置驗證：dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1；dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1。

## 實作狀態更新（2026-05-15）

- 已完成：完成餐廳/評論/排行責任邊界整理，將 Restaurant、Review、Ranking、Import Target 的 service 與 repository 拆分。

- 已完成：導入輕量資料庫 migration 基礎，新增 schema_migrations 與版本化 DatabaseMigrations，後續 schema 變更可用 migration 追蹤。

- 部分完成：餐廳資料欄位已擴充 BranchName、City、District、Latitude、Longitude、OpeningHours、PriceMin、PriceMax、CuisineType、Tags、Description、OfficialUrl、GoogleMapUrl。尚未建立獨立城市/行政區/料理分類資料表。

- 部分完成：餐廳搜尋已支援 keyword、status、city、district、cuisineType、priceMin、priceMax、minScore、page、pageSize。尚未支援距離搜尋、附近餐廳、Typesense typo tolerance。

- 部分完成：餐廳排序已支援 latest、ranking、reviewCount、favoriteCount；排行榜與列表排序已使用 approved / non-suspicious / non-deleted reviews 的 Bayesian 平台分數，並納入收藏數 FavoriteScore 訊號。尚未納入照片品質、近期熱度與使用者權重。

- 部分完成：評論模型已支援 TasteScore、ServiceScore、EnvironmentScore、ValueScore、RevisitScore、Content、VisitDate、PricePerPerson、DiningType、CompanionType、狀態欄位、會員歸屬與 30 天重複評論限制，並已支援匿名評論檢舉。尚未實作評論照片與會員權重。

- 已完成：後台評論審核 API 已支援評論列表查詢、狀態更新、批次狀態更新、可疑標記、刪除標記、審核原因紀錄、操作紀錄查詢、審核紀錄全域搜尋、檢舉列表查詢與檢舉處理，且已加入 Admin role JWT 授權保護。

- 部分完成：自動反作弊 MVP 已支援同評論者短時間大量評論、同餐廳重複內容、低品質內容、與既有餐廳平均分差距過大的規則式偵測；命中後自動標記 is_suspicious、狀態設為 Suspicious，並保存 suspicious_reason 與 suspicious_detected_at。尚未支援 IP/裝置指紋、會員信任分數與批次重算。

- 已完成：會員評論歸屬與 30 天限制已落地，登入會員新增完整評論時會從 User JWT 寫入 restaurant_reviews.user_id，且同會員同餐廳 30 天內不可重複新增有效評論。

- 部分完成：會員系統基礎已支援 users 資料表、一般會員註冊/登入、PBKDF2 密碼雜湊與 User JWT。尚未實作會員資料維護、Refresh Token 與 email 驗證。

- 已完成：後台登入/授權已支援第一位管理員 bootstrap、PBKDF2 密碼雜湊、JWT 登入、Admin Refresh Token 輪替與撤銷、Admin API 授權限制、管理員列表、啟用/停用管理員、目前登入管理員改密碼、角色更新與 ReviewModerator 權限細分，且審核操作可關聯管理員。

- 部分完成：Next.js 前台 FoodTrust.Web 已建立，支援首頁、餐廳列表、餐廳詳細、登入、註冊、我的收藏、收藏操作與新增評論的第一版流程。

仍未完成：照片上傳、地圖/距離搜尋、Typesense、Redis、商家認領、SEO 頁面。

- 本次建置驗證：dotnet build FoodTrust.Api\\FoodTrust.Api.csproj --no-restore -m:1；dotnet build FoodTrust.Worker\\FoodTrust.Worker.csproj --no-restore -m:1。

## 實作狀態更新（2026-05-18）

- 已完成：登入會員新增評論會套用 User role JWT 授權，從 ClaimTypes.NameIdentifier 寫入 restaurant_reviews.user_id。

- 已完成：新增同會員同餐廳 30 天內不可重複新增評論的服務層檢查，避免重複評論進入評分資料。

- 已完成：JWT 驗證設定改為同時接受 AdminJwt 與 UserJwt issuer/audience，避免會員 token 在受保護 API 被拒。

- 已完成：收藏功能基礎已落地，新增 favorite_restaurants 資料表，支援會員收藏/取消收藏餐廳與查詢我的收藏餐廳分頁列表。

- 已完成：後台管理員列表與啟用/停用管理已落地，新增 GET /api/v1/admin/users 與 PATCH /api/v1/admin/users/{id}/active，並避免管理員停用自己。

- 已完成：目前登入管理員改密碼已落地，新增 PATCH /api/v1/admin/users/me/password，會驗證目前密碼、新密碼長度與帳號啟用狀態。

- 已完成：後台評論批次審核已落地，新增 PATCH /api/v1/admin/reviews/status，可一次更新多筆評論狀態並為每筆建立審核紀錄，回傳成功筆數與找不到的評論 ID。

- 已完成：後台審核紀錄搜尋已落地，新增 GET /api/v1/admin/reviews/moderation-logs，支援 reviewId、adminUserId、action、from、to、page、pageSize 篩選。

- 已完成：Admin Refresh Token 已落地，登入會簽發 refresh token，新增 POST /api/v1/admin/auth/refresh 輪替 access/refresh token，以及 POST /api/v1/admin/auth/revoke 撤銷 refresh token。

- 已完成：後台權限細分已落地，新增 ReviewModerator 角色與 Admin.ReviewModeration policy，評論審核/檢舉 API 開放 Admin 或 ReviewModerator 使用，管理員管理仍限 Admin，並新增 PATCH /api/v1/admin/users/{id}/role 更新角色。

- 已完成：排行收藏訊號整合已落地，餐廳列表與排行榜回傳 favoriteCount，列表支援 sortBy=favoriteCount，排行榜分數納入 FavoriteScore 5% 權重。

- 已完成：FoodTrust.Web 前台骨架已建立，使用 TypeScript + Next.js App Router + Tailwind CSS，已完成首頁、餐廳搜尋列表、餐廳詳細、會員登入/註冊、我的收藏、收藏按鈕與評論表單第一版。

- 目前推送進度（2026-05-18）：後台管理補強已完成至權限細分，排行已整合收藏訊號，FoodTrust.Web 前台 MVP 骨架已建立；本版準備推送 main 至 origin/main。

- 已完成：FoodTrust.Web 前端互動改善已落地，新增會員狀態列與登出、餐廳列表分頁控制，並在評論送出成功後刷新餐廳詳細頁資料。

- 已完成：FoodTrust.Web 收藏狀態同步已落地，收藏列表會同步本機會員收藏狀態，餐廳詳情頁收藏按鈕會依狀態初始化並在切換後刷新資料。

- 已完成：FoodTrust.Web API 錯誤處理已補強，前端會解析後端錯誤 payload，登入、註冊、收藏、評論與收藏列表可顯示更精準的失敗訊息。

- 已完成：FoodTrust.Web SEO route 與視覺細節已補強，新增頁面 metadata、餐廳詳情動態 metadata、robots.txt、sitemap.xml、焦點樣式與餐廳外部連結。

- 已完成：FoodTrust.Web 部署設定文件已補強，新增 .env.example、更新 README，記錄 NEXT_PUBLIC_API_BASE_URL、NEXT_PUBLIC_SITE_URL 與建置部署指令。

- 已完成：FoodTrust.Web 前端驗證流程已補強，新增 typecheck 與 verify npm script，並建立 GitHub Actions workflow 自動執行 lint、typecheck、build。

- 已完成：AWS Amplify Hosting 部署準備已補強，新增 monorepo amplify.yml，並於 FoodTrust.Web README 記錄 App root、build artifact、cache 與 NEXT_PUBLIC_API_BASE_URL / NEXT_PUBLIC_SITE_URL 設定。

- 目前 AWS 部署進度（2026-05-18）：AWS 帳號 root MFA 已完成，已建立 IAM Identity Center 使用者 jay-admin 並啟用 MFA，已指派 AdministratorAccess；已建立每月 10 USD 預算警示。Amplify 已連接 GitHub repo sujay06297/FoodTrust main 分支並完成首次前端部署，Amplify 網址為 https://main.d2p8rcvp0wb9cp.amplifyapp.com；目前待確認項目為該網址 DNS/邊緣節點生效狀態，若仍無法開啟，下一步檢查 nslookup、curl -I、Amplify Deploy/Verify log，必要時重新部署此版本。

- 本次建置驗證：dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1；dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1。

## 開發接續摘要（給新視窗/新對話快速讀取，2026-05-15）

請下一位助理先讀本段，再讀「實作狀態更新」。目前工作目標：依 MVP 優先順序補完 FoodTrust 美食排行網後端，規格書要隨功能同步更新。

## 目前專案位置與技術架構

工作目錄：C:\Users\User\FoodTrust。方案包含 FoodTrust.Api、FoodTrust.Core、FoodTrust.Infrastructure、FoodTrust.Worker。

架構原則：Core 放 domain/application 介面、模型與 service；Infrastructure 放 Dapper/MySQL repository、migration、外部匯入；Api 放 Controllers、request models、JWT 設定；Worker 放背景匯入。Core 的 interface 由 Infrastructure 實作是正常分層。

資料庫使用 MySQL，migration 由 FoodTrust.Infrastructure\Data\DatabaseMigrations.cs 與 DatabaseInitializer 啟動時套用，schema_migrations 記錄版本。

## 目前已完成的後端功能

餐廳：新增、查詢、詳細、更新、狀態更新、FDA 食品業者匯入、來源去重、匯入紀錄查詢。

搜尋/排行：支援 keyword、status、city、district、cuisineType、priceMin、priceMax、minScore、page、pageSize；排序支援 latest、ranking、reviewCount；排行與列表使用 approved/non-suspicious/non-deleted reviews 的 Bayesian 平台分數。

評論：五項分數 Taste/Service/Environment/Value/Revisit、平均分、內容、用餐日期、人均消費、用餐型態、同行型態、狀態、可疑/刪除旗標。

會員/後台：一般會員註冊/登入、User JWT、第一位管理員 bootstrap、PBKDF2 密碼雜湊、Admin JWT、Admin role 授權、評論審核列表、狀態更新、可疑標記、刪除標記、審核原因、操作紀錄查詢。

檢舉與反作弊：公開端匿名評論檢舉、後台檢舉列表、檢舉狀態處理、處理管理員與處理備註；新增完整評論時會執行規則式反作弊，命中後自動標記 Suspicious 並保存可疑原因。

## 近期本機 commits

cc46d92 新增評論檢舉流程；ae72ee9 新增後台評論審核紀錄；74c9e12 新增後台登入與授權基礎；61b185f 新增後台評論審核 API；e779222 新增中文方法註解。這些可能尚未 push，請先用 git status/log 確認。

## 建置與操作注意事項

- 建置請用單執行緒避免 Windows DLL lock：dotnet build FoodTrust.Api\FoodTrust.Api.csproj --no-restore -m:1；dotnet build FoodTrust.Worker\FoodTrust.Worker.csproj --no-restore -m:1。

- Git 不一定在 PATH，常用路徑：C:\Users\User\AppData\Local\Fork\gitInstance\2.50.1\cmd\git.exe。

- PowerShell Get-Content 可能讓 UTF-8 中文顯示成亂碼；不要直接判定檔案壞掉。讀中文可用 rg 或指定 UTF8。

## 下一步建議優先順序

1. 產品功能後續：照片上傳、地圖/距離搜尋、Typesense、Redis、Next.js 前台、SEO 頁面、商家認領。

2. 前端 MVP 後續：已完成，前端改以 Vercel 為部署目標；後端 API 規劃改放 Google Cloud Run，資料庫規劃改用 TiDB Cloud Starter。

3. 產品功能後續：照片上傳、地圖/距離搜尋、Typesense、Redis、Next.js 前台、SEO 頁面、商家認領。

4. 持續優化反作弊：補 IP/裝置指紋、會員信任分數、人工審核回饋與批次重算。

5. 前端 MVP：在後端核心流程穩定後，開始規劃 Next.js 前台餐廳列表、詳細頁、排行與會員互動。

## 實作時同步規則

每完成一個功能 slice，請更新本規格書的實作狀態與本段接續摘要，然後 build Api/Worker，最後 commit。使用者偏好中文註解與中文說明。

## AWS / 前端部署接續狀態更新（2026-05-19）

- 已完成：本機 FoodTrust.Web 前端已成功啟動，網址為 http://127.0.0.1:3000；本機新增 .env.local，設定 NEXT_PUBLIC_API_BASE_URL=http://localhost:5000、NEXT_PUBLIC_SITE_URL=http://localhost:3000。若後端 API 未啟動，首頁、餐廳列表與餐廳詳細頁可能因 API 無回應而載入失敗；登入頁可正常開啟。

- 已完成：為處理 Amplify 預設網域 404，已將 FoodTrust.Web 的 Next.js 從 16.2.6 調整為 Amplify SSR 支援範圍內的 15.5.18，並同步修正 eslint-config-next 與 ESLint flat config。已通過本機 lint、typecheck 與 next build。

- 已完成：已提交並推送 commit 7b0ba43「修正 Amplify 前端部署 404」到 origin/main。Amplify 部署記錄可看到最新 commit 與 Next.js 15.5.18。

- 目前 AWS 狀態：Amplify App ID 為 d2p8rcvp0wb9cp，區域為 ap-southeast-2（亞太地區/雪梨），預設網域為 https://main.d2p8rcvp0wb9cp.amplifyapp.com。CloudFront/HTTP 有回應，但 HTTPS 網頁目前仍顯示 HTTP ERROR 404。

- 目前 AWS 狀態：已在 CloudShell 成功執行 update-app，App platform 已顯示為 WEB_COMPUTE。這代表 Amplify App 已切到 Next.js SSR/Compute 類型。

- 目前阻塞：部署 5 的 build 階段成功，但部署階段失敗，錯誤為「Failed to find the deploy-manifest.json file in the build output」。判斷原因偏向 monorepo 設定未讓 Amplify SSR 正確辨識 FoodTrust.Web 為 App root。

- 下次接續第一步：在 AWS CloudShell 執行 aws amplify update-branch --app-id d2p8rcvp0wb9cp --branch-name main --environment-variables AMPLIFY_MONOREPO_APP_ROOT=FoodTrust.Web --region ap-southeast-2

- 下次接續第二步：接著執行 aws amplify start-job --app-id d2p8rcvp0wb9cp --branch-name main --job-type RELEASE --region ap-southeast-2，等待新部署完成後再測 https://main.d2p8rcvp0wb9cp.amplifyapp.com。

- 若仍失敗：檢查 Amplify「託管 > 環境變數」是否存在 AMPLIFY_MONOREPO_APP_ROOT=FoodTrust.Web；檢查「託管 > 重寫和重新引導」是否建立 SSR rewrite；必要時考慮重新建立 Amplify App 並在建立時明確指定 monorepo App root 為 FoodTrust.Web。

## AWS 資源退場與停止計費清單（2026-06-10）

- 背景：目前前端已不打算繼續放在 AWS。此段用來記錄已知 AWS 資源、帳單檢查結果與退場步驟，避免未使用資源持續產生費用。

- 已知前端部署資源：AWS Amplify Hosting，App 名稱 FoodTrust，App ID d2p8rcvp0wb9cp，區域 ap-southeast-2（亞太地區/雪梨），分支 main，預設網域 https://main.d2p8rcvp0wb9cp.amplifyapp.com。

- 退場主要動作：若不再使用 AWS 前端，應至 AWS Amplify > FoodTrust > 應用程式設定 > 一般設定，執行 Delete app。刪除後 amplifyapp.com 預設網址會失效，Amplify Hosting 建置與託管費用應停止。

- 帳單現況：Billing and Cost Management 顯示本月至今約 0.03 USD，Cost breakdown 曾出現 AWS Amplify、Amazon S3、AWS Secrets Manager、AWS Glue、Tax、Others 等分類。金額很低，但仍需逐項確認是否存在可刪資源。

- 本專案程式碼依賴判斷：目前程式碼與設定檔只確認有 Amplify 前端部署設定（amplify.yml 與 FoodTrust.Web README）。尚未看到 Secrets Manager 或 Glue 的程式碼依賴。S3 僅在原規格書技術架構中作為未來照片物件儲存建議，尚未看到本專案已實作或必須保留的 S3 整合。

- 停止計費檢查清單：1. Amplify：確認 FoodTrust App 已刪除。2. S3：檢查是否有不需要的 bucket；不用時先清空再刪除 bucket。3. Secrets Manager：檢查是否有 secrets；不用時排程刪除，避免按 secret 持續計費。4. AWS Glue：檢查 crawler、job、database、table；未使用時刪除。5. Route 53：若有 hosted zone 或 DNS record 指向前端，確認是否需要保留；Hosted Zone 可能按月計費。6. CloudFront：若有 distribution，先 Disable，完成後 Delete。7. EC2-Other：檢查 EBS volume、snapshot、Elastic IP、Load Balancer，未使用時刪除或釋放。

- 區域檢查：Amplify 已知在 ap-southeast-2，但帳單頁是全球彙總。刪除資源時需切換到實際區域檢查，S3 也需看全帳號 bucket 清單。

- 後續維運規則：每次新增 AWS 服務，都要在本規格書記錄服務名稱、區域、用途、是否必要、刪除方式與可能費用來源。若只是短期測試，測試完成後要在 Billing > Bills / Cost Explorer 確認沒有殘留費用。

- 目前建議：若前端改放其他平台或只保留本機開發，先刪 Amplify App，再檢查 S3、Secrets Manager、Glue 是否有實際資源；確認沒有資源後保留 10 USD 預算警示作為防呆。

## 新雲端部署決策（2026-06-10）

- 部署方向：前端不再放 AWS Amplify，改以低成本、可逐步擴充的 serverless 方案為主。

- 前端部署：`FoodTrust.Web` 規劃部署至 Vercel Hobby。理由是 Next.js 支援完整、部署流程簡單，且前端低流量階段成本較容易控制。

- API 部署：`FoodTrust.Api` 規劃部署至 Google Cloud Run。理由是可用 Docker 部署 ASP.NET Core API、支援 scale to zero、低流量時成本較低，且朋友已有 Cloud Run 部署經驗可參考。

- Worker 部署：`FoodTrust.Worker` 規劃改以 Cloud Run Jobs 或等效排程工作執行。理由是匯入/批次工作不需要長時間常駐，適合用 job 型態降低閒置成本。

- 資料庫部署：資料庫規劃使用 TiDB Cloud Starter。理由是目前專案偏 MySQL 架構，TiDB 提供 MySQL 相容介面，較 Supabase / Neon 這類 PostgreSQL 平台需要的程式改動少，且 Starter 免費額度適合 MVP 階段。

- 暫不採用：Supabase 作為第一階段 DB。Supabase 本身可用，但它是 PostgreSQL，若採用需調整 NuGet driver、connection string、migration SQL、AUTO_INCREMENT、日期/布林型別與 repository 查詢語法。除非後續決定轉 PostgreSQL，否則先不列為首選。

- 暫不採用：Google Cloud SQL / Azure MySQL 作為第一階段 DB。兩者穩定但較容易產生固定費或免費期限到期成本，與目前「便宜優先」目標不完全一致。

- 成本控制規則：Cloud Run API 應設定 min instances = 0、max instances 先設 1、低 CPU/Memory 起步，並建立 GCP budget alert。TiDB Cloud 需設定用量提醒，避免超過免費額度後未察覺。

- 後續實作項目：新增 `FoodTrust.Api` Dockerfile、規劃 `FoodTrust.Worker` job 部署方式、補 Cloud Run / TiDB 部署文件、整理正式環境變數清單，並確認 migration 可在 TiDB Cloud Starter 正常執行。

## TiDB / GCP 接續狀態更新（2026-06-12）

- 已完成：TiDB Cloud Starter 已建立 `FoodTrust-DEV` 實例，FDA 候選資料匯入流程已可寫入 `candidate_restaurants`。
- 已完成：匯入流程改為「先進候選表、人工審核後再寫入 restaurants」，並新增候選餐廳 API、後台頁面 `/admin/candidate-restaurants`、Approve / Reject 流程與 Google Search 按鈕。
- 已完成：`FoodTrust.Worker` 已補 `launchSettings.json`、`appsettings*.json` 複製規則、TiDB CA / TLS / BootstrapDatabase 設定欄位，且已加回 solution。
- 已完成：`FoodTrust.Web` API client 已改為必須明確設定 `NEXT_PUBLIC_API_BASE_URL`，不再 fallback 到本機 `localhost:5000`。
- 已完成：新增 `FoodTrust.Api/Dockerfile`、`.dockerignore`、`.gcloudignore`、`cloudbuild.api.yaml`，可使用 Cloud Build 建置 API image。
- 已完成：GCP 專案 `foodtrust-dev`、Artifact Registry `foodtrust`、必要 API（Cloud Run Admin、Artifact Registry、Secret Manager、Cloud Scheduler、Cloud Build）均已建立或啟用。
- 已完成：Cloud Build 已成功將 API image 推送到 `asia-east1-docker.pkg.dev/foodtrust-dev/foodtrust/foodtrust-api:dev`。
- 已完成：API 已補 CORS policy，預設允許 `http://localhost:3000` 與 `http://127.0.0.1:3000`。
- 目前阻塞：Cloud Run `foodtrust-api` 首次 deploy 失敗，錯誤表面為容器未在 `PORT=8080` 上啟動；高機率根因是啟動時先執行 `DatabaseInitializer.InitializeAsync()`，但 Cloud Run 尚未提供正確的 TiDB connection string / JWT / CORS 正式設定，導致容器在 listen 前 crash。
- 下次接續第一步：用最新版 image（含 CORS）再執行一次 `gcloud builds submit --config cloudbuild.api.yaml .`，確保 tag `foodtrust-api:dev` 為最新版本。
- 下次接續第二步：建立 Secret Manager secrets，至少包含 TiDB connection string、AdminJwt signing key、UserJwt signing key。
- 下次接續第三步：用 `gcloud run deploy ... --set-secrets ...` 重新部署 `foodtrust-api`，確認 Cloud Run URL 可對外提供 API。
- 下次接續第四步：把 `FoodTrust.Web/.env.local` 的 `NEXT_PUBLIC_API_BASE_URL` 改成 Cloud Run DEV URL，驗證本機前端直接打 DEV API。
- 下次接續第五步：評估 `FoodTrust.Worker` 改為 Cloud Run Jobs + Cloud Scheduler，取代目前長駐 worker。
