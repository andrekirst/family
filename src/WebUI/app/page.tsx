"use client";

import type { Metadata } from "next";
import { signOut } from "next-auth/react";

export default function Page() {
  return (
    <>
      <button onClick={() => signOut()}>Abmelden</button>
    </>
  );
}