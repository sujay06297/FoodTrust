import type { Metadata } from "next";
import { SiteHeader } from "@/components/layout/site-header";
import "./globals.css";

export const metadata: Metadata = {
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000"),
  title: {
    default: "FoodTrust 食信",
    template: "%s | FoodTrust 食信",
  },
  description: "可信賴的美食排行與餐廳評價平台",
  openGraph: {
    title: "FoodTrust 食信",
    description: "可信賴的美食排行與餐廳評價平台",
    locale: "zh_TW",
    siteName: "FoodTrust 食信",
    type: "website",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="zh-Hant" className="h-full antialiased">
      <body className="min-h-full bg-zinc-50 text-zinc-950">
        <SiteHeader />
        {children}
      </body>
    </html>
  );
}
