"use client";

import { Menu, MenuButton, MenuItem, MenuItems } from "@headlessui/react";
import { ChevronDownIcon } from "@heroicons/react/24/outline";
import { useSession } from "next-auth/react";
import Image from "next/image";

export default function UserNavigation() {
    const { data: session } = useSession();

    const userNavigation = [
        {
            name: "Profil",
            href: "/user/profile"
        },
        {
            name: "Abmelden",
            href: "/user/logout"
        }
    ];

    return (
        <Menu as="div" className="relative">
            <MenuButton className="-m-1.5 flex items-center p-1.5">
                <span className="sr-only">Open user menu</span>
                <Image
                    alt=""
                    src="https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80"
                    className="h-8 w-8 rounded-full bg-gray-50"
                    width="32"
                    height="32"
                />
                <span className="hidden lg:flex lg:items-center">
                    <span aria-hidden="true" className="ml-4 text-sm font-semibold leading-6 text-gray-900">
                        {
                            session?.user?.name
                        }
                    </span>
                    <ChevronDownIcon aria-hidden="true" className="ml-2 h-5 w-5 text-gray-400" />
                </span>
            </MenuButton>
            <MenuItems
                transition
                className="absolute right-0 z-10 mt-2.5 w-32 origin-top-right rounded-md bg-white py-2 shadow-lg ring-1 ring-gray-900/5 transition focus:outline-none data-[closed]:scale-95 data-[closed]:transform data-[closed]:opacity-0 data-[enter]:duration-100 data-[leave]:duration-75 data-[enter]:ease-out data-[leave]:ease-in"
                >
                {userNavigation.map((item) => (
                <MenuItem key={item.name}>
                    <a href={item.href} className="block px-3 py-1 lg:text-base sm:text-lg leading-6 text-gray-900 data-[focus]:bg-gray-50">
                        {item.name}
                    </a>
                </MenuItem>
                ))}
            </MenuItems>
        </Menu>
    );
}