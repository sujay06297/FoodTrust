# FoodTrust.Web

FoodTrust 前台網站，使用 TypeScript、Next.js App Router、React 與 Tailwind CSS。

## 環境需求

- Node.js 20+
- npm
- FoodTrust.Api 可連線的 API Base URL

## 環境變數

複製 `.env.example` 為 `.env.local` 後依環境調整：

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
NEXT_PUBLIC_SITE_URL=http://localhost:3000
```

變數用途：

- `NEXT_PUBLIC_API_BASE_URL`：前台呼叫後端 API 的 base URL。
- `NEXT_PUBLIC_SITE_URL`：產生 metadata、robots.txt、sitemap.xml 使用的網站網址。

## 本機開發

```bash
npm install
npm run dev
```

預設開發網址為 `http://localhost:3000`。

## 驗證

```bash
npm run lint
npm run build
```

`npm run build` 會檢查 TypeScript、產生 Next.js production build，並確認 `/robots.txt` 與 `/sitemap.xml` route 可輸出。

## 部署

部署平台需設定：

- Install command：`npm ci`
- Build command：`npm run build`
- Start command：`npm run start`
- Output：Next.js 預設輸出

正式環境請將 `NEXT_PUBLIC_API_BASE_URL` 指到公開 API 網址，並將 `NEXT_PUBLIC_SITE_URL` 設為前台正式網域。
