import { classNames } from "@/app/lib/string";
import { CalendarDaysIcon, HomeIcon } from "@heroicons/react/24/outline";
import Link from "next/link";
import { usePathname } from "next/navigation";

interface NavigationItem {
    name: string,
    href: string,
    icon: any,
    current: boolean
}

type Func<T> = (args: T) => boolean;

export default function Navigation() {

    const pathname = usePathname();

    console.log('pathname', pathname);

    const navigation: NavigationItem[] = [
        {
          name: "Dashboard",
          href: "/",
          icon: HomeIcon,
          current: false
        },
        {
          name: "Kalender",
          href: "/calendar/me/list",
          icon: CalendarDaysIcon,
          current: false
        }
      ];

    navigation.forEach((item) => {
        if(item.href === pathname) {
            item.current = true;
        }

        // TODO Dynamisch gestalten
        if(item.name === 'Kalender' && pathname.startsWith('/calendar')) {
            item.current = true;
        }
    })

    return (
        <li>
            <ul role="list" className="-mx-2 space-y-1">
                {navigation.map((item) => (
                <li key={item.name}>
                    <Link href={item.href}>
                        <button
                        className={classNames(
                            item.current
                            ? 'bg-gray-50 text-indigo-600'
                            : 'text-gray-700 hover:bg-gray-50 hover:text-indigo-600',
                            'group flex gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                        )}
                        >
                        <item.icon
                            aria-hidden="true"
                            className={classNames(
                            item.current ? 'text-indigo-600' : 'text-gray-400 group-hover:text-indigo-600',
                            'h-6 w-6 shrink-0',
                            )}
                        />
                        {item.name}
                        </button>
                    </Link>
                    
                </li>
                ))}
            </ul>
            </li>
    )
}