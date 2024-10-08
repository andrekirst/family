import { classNames } from "@/app/lib/string";
import { HomeIcon, UsersIcon } from "@heroicons/react/24/outline";

export default function Navigation() {
    const navigation = [
        {
          name: "Dashboard",
          href: "/",
          icon: HomeIcon,
          current: true
        },
        {
          name: "Familie",
          href: "/",
          icon: UsersIcon,
          current: false
        }
      ];

    return (
        <li>
            <ul role="list" className="-mx-2 space-y-1">
                {navigation.map((item) => (
                <li key={item.name}>
                    <a
                    href={item.href}
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
                    </a>
                </li>
                ))}
            </ul>
            </li>
    )
}