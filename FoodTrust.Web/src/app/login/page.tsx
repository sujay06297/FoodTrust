import Link from "next/link";
import { AuthForm } from "@/components/auth/auth-form";

export default function LoginPage() {
  return (
    <main className="px-4 py-10">
      <AuthForm mode="login" />
      <p className="mt-4 text-center text-sm text-zinc-600">
        還沒有帳號？{" "}
        <Link className="font-medium text-zinc-950" href="/register">
          註冊會員
        </Link>
      </p>
    </main>
  );
}
