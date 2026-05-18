import Link from "next/link";
import { AuthForm } from "@/components/auth/auth-form";

export default function RegisterPage() {
  return (
    <main className="px-4 py-10">
      <AuthForm mode="register" />
      <p className="mt-4 text-center text-sm text-zinc-600">
        已經有帳號？{" "}
        <Link className="font-medium text-zinc-950" href="/login">
          登入
        </Link>
      </p>
    </main>
  );
}
