# ADR 0001：雲端部署改為 Vercel + Google Cloud Run + TiDB Cloud

- 狀態：已採用（2026-06-10）
- 相關：[../ops/aws-teardown.md](../ops/aws-teardown.md)、[../devlog.md](../devlog.md) 2026-05-19 / 2026-06-10 / 2026-06-12 項目

## 背景

前端原本嘗試部署在 AWS Amplify Hosting（見 devlog 2026-05-19），但 monorepo SSR 設定一直卡在「Failed to find the deploy-manifest.json file in the build output」，判斷是 Amplify 對 monorepo App root 的辨識問題。同時盤點 AWS 帳單與資源，決定前端不再繼續走 AWS 路線。

需要重新選擇：前端 hosting、API hosting、Worker 執行方式、資料庫。目標是「便宜優先、MVP 階段成本可控、盡量不改動既有 MySQL 導向的程式碼」。

## 決策

- 前端 `FoodTrust.Web` → **Vercel Hobby**。Next.js 官方支援完整、部署流程簡單，低流量階段成本容易控制。
- API `FoodTrust.Api` → **Google Cloud Run**。可用 Docker 部署 ASP.NET Core、支援 scale to zero、低流量成本較低，且有團隊成員的 Cloud Run 部署經驗可參考。
- Worker `FoodTrust.Worker` → 規劃改用 **Cloud Run Jobs**（或等效排程工作），取代長駐 worker。匯入/批次工作不需要長時間常駐，適合 job 型態降低閒置成本。
- 資料庫 → **TiDB Cloud Starter**。專案偏 MySQL 架構，TiDB 提供 MySQL 相容介面，較 Supabase/Neon 這類 PostgreSQL 平台需要的程式改動少，且 Starter 免費額度適合 MVP 階段。

## 考慮過的替代方案

| 方案 | 不採用原因 |
| --- | --- |
| 繼續用 AWS Amplify Hosting | monorepo SSR 部署一直失敗，且已決定前端退出 AWS |
| Supabase（PostgreSQL） | 需調整 NuGet driver、connection string、migration SQL、AUTO_INCREMENT、日期/布林型別與 repository 查詢語法；除非後續決定轉 PostgreSQL，否則改動成本不划算 |
| Google Cloud SQL / Azure MySQL | 穩定但容易產生固定費或免費期限到期成本，與「便宜優先」目標不完全一致 |

## 後果

- 需要新增 `FoodTrust.Api` Dockerfile、Cloud Build 設定、Secret Manager 管理正式環境變數（TiDB connection string、JWT signing key）。
- 需要規劃 Cloud Run API 的 min instances = 0、max instances 限制與 GCP budget alert，避免超出免費額度後未察覺。
- 原本規格書 12. 節「技術架構建議」表格中的 MySQL/PostgreSQL、S3 等建議與此決策不完全一致，已在規格書該節加註說明，並以此 ADR 與 [handoff.md](../handoff.md) 為現況依據。
- AWS 既有資源需要退場，退場清單見 [ops/aws-teardown.md](../ops/aws-teardown.md)。
- 執行過程中在 Cloud Run 部署遇到容器未在 `PORT=8080` 啟動的阻塞（見 devlog 2026-06-12、[handoff.md](../handoff.md)），截至本 ADR 撰寫時尚未解除。
