"use client";

import { useSession } from "next-auth/react";

export default function UserProfilePage() {

    const { data: session } = useSession();

    return (
        <>
            <h1>Hallo {session?.user?.name}</h1>
        </>
    );
}