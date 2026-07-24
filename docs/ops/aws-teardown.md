# AWS 資源退場與停止計費清單

> 背景：前端已不打算繼續放在 AWS（改走 Vercel + Google Cloud Run + TiDB Cloud，見 [../devlog.md](../devlog.md) 2026-06-10 項目）。此檔案記錄已知 AWS 資源、帳單檢查結果與退場步驟，避免未使用資源持續產生費用。這是一份操作清單，會隨每次檢查更新狀態，不是歷史日誌。

最後檢查日期：2026-06-10

## 已知資源

- **AWS Amplify Hosting** — App 名稱 FoodTrust，App ID `d2p8rcvp0wb9cp`，區域 ap-southeast-2（亞太地區/雪梨），分支 main，預設網域 `https://main.d2p8rcvp0wb9cp.amplifyapp.com`。

## 退場主要動作

若不再使用 AWS 前端：AWS Amplify → FoodTrust → 應用程式設定 → 一般設定 → 執行 **Delete app**。刪除後 `amplifyapp.com` 預設網址會失效，Amplify Hosting 建置與託管費用應停止。

## 帳單現況（2026-06-10 檢查時）

Billing and Cost Management 顯示本月至今約 0.03 USD。Cost breakdown 曾出現 AWS Amplify、Amazon S3、AWS Secrets Manager、AWS Glue、Tax、Others 等分類，金額很低，但仍需逐項確認是否存在可刪資源。

程式碼依賴判斷：目前程式碼與設定檔只確認有 Amplify 前端部署設定（`amplify.yml` 與 `FoodTrust.Web` README）。尚未看到 Secrets Manager 或 Glue 的程式碼依賴。S3 僅在規格書技術架構章節作為未來照片物件儲存的建議，尚未看到本專案已實作或必須保留的 S3 整合。

## 停止計費檢查清單

1. **Amplify**：確認 FoodTrust App 已刪除。
2. **S3**：檢查是否有不需要的 bucket；不用時先清空再刪除 bucket。
3. **Secrets Manager**：檢查是否有 secrets；不用時排程刪除，避免按 secret 持續計費。
4. **AWS Glue**：檢查 crawler、job、database、table；未使用時刪除。
5. **Route 53**：若有 hosted zone 或 DNS record 指向前端，確認是否需要保留；Hosted Zone 可能按月計費。
6. **CloudFront**：若有 distribution，先 Disable，完成後 Delete。
7. **EC2-Other**：檢查 EBS volume、snapshot、Elastic IP、Load Balancer，未使用時刪除或釋放。

## 區域檢查提醒

Amplify 已知在 ap-southeast-2，但帳單頁是全球彙總。刪除資源時需切換到實際區域檢查，S3 也需看全帳號 bucket 清單。

## 後續維運規則

每次新增 AWS 服務，都要在本檔記錄服務名稱、區域、用途、是否必要、刪除方式與可能費用來源。若只是短期測試，測試完成後要在 Billing > Bills / Cost Explorer 確認沒有殘留費用。

## 目前建議

若前端改放其他平台或只保留本機開發：先刪 Amplify App，再檢查 S3、Secrets Manager、Glue 是否有實際資源；確認沒有資源後保留 10 USD 預算警示作為防呆。
