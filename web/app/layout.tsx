import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "HanEn Cursor Indicator",
  description: "A Windows cursor-side Korean/English input indicator with mascot packs and lifetime licensing."
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="ko">
      <body>
        <header className="shell nav">
          <div className="brand">HanEn Cursor Indicator</div>
          <nav className="navLinks" aria-label="Main">
            <a href="/">Home</a>
            <a href="/account">Account</a>
            <a href="/privacy">Privacy</a>
            <a href="/terms">Terms</a>
          </nav>
        </header>
        {children}
      </body>
    </html>
  );
}
