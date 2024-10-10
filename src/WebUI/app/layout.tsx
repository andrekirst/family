'use client';

import "./globals.css";
import AuthProvider from "@/utils/SessionProvider";
import Home from "./ui/home";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="h-full bg-white">
      <body className="h-full">
        <AuthProvider>
          <Home children={children} />
        </AuthProvider>
      </body>
    </html>
  );
}
