import type { Metadata } from "next";
import { SiteHeader } from "@/components/layout/site-header";
import "./globals.css";

export const metadata: Metadata = {
  title: "FoodTrust 食信",
  description: "可信賴的美食排行與餐廳評價平台",
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
