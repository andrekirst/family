"use client";

import { classNames } from "@/app/lib/string";
import { getFamilyMembers, getFamilyMembersData } from "@/services/api/sidebar";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { useEffect, useState } from "react";

export default function FamilyMembers() {
    const [familyMembers, setFamilyMembers] = useState<getFamilyMembersData[]>([]);

    const { data: session } = useSession();

    useEffect(() => {
        const fetchData = async () => {
            const token = session?.idToken;
            const result = await getFamilyMembers(token!);
            setFamilyMembers(result);
        }

        fetchData();
    }, []);

    return (
        <li>
            <div className="text-xs font-semibold leading-6 text-gray-400">Your teams</div>
            <ul role="list" className="-mx-2 mt-2 space-y-1">
                {familyMembers.map((familyMember) => (
                <li key={familyMember.displayText}>
                    <Link
                        href={familyMember.href}
                        className={classNames(
                            familyMember.current
                            ? 'bg-gray-50 text-indigo-600'
                            : 'text-gray-700 hover:bg-gray-50 hover:text-indigo-600',
                            'group flex gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                        )}
                        >
                        <span
                            className={classNames(
                                familyMember.current
                                ? 'text-indigo-600'
                                : 'text-gray-400 group-hover:text-indigo-600',
                            'flex h-6 w-6 shrink-0 items-center justify-center rounded-lg bg-white text-[0.625rem] font-medium',
                            )}
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-6">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
                            </svg>

                        </span>
                        <span className="truncate">{familyMember.displayText}</span>
                    </Link>
                </li>
                ))}
            </ul>
            </li>
    );
}