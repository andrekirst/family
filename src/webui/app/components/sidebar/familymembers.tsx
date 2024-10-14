import { classNames } from "@/app/lib/string";

export default function FamilyMembers() {
    const familienmitglieder = [
        { id: 1, name: 'André', href: '#', initial: '1', current: false },
        { id: 2, name: 'Alwina', href: '#', initial: '2', current: false },
        { id: 3, name: 'Annika', href: '#', initial: '3', current: false },
        { id: 4, name: 'Anneli', href: '#', initial: '4', current: false }
    ];

    return (
        <li>
            <div className="text-xs font-semibold leading-6 text-gray-400">Your teams</div>
            <ul role="list" className="-mx-2 mt-2 space-y-1">
                {familienmitglieder.map((familienmitglied) => (
                <li key={familienmitglied.name}>
                    <a
                    href={familienmitglied.href}
                    className={classNames(
                        familienmitglied.current
                        ? 'bg-gray-50 text-indigo-600'
                        : 'text-gray-700 hover:bg-gray-50 hover:text-indigo-600',
                        'group flex gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                    )}
                    >
                    <span
                        className={classNames(
                        familienmitglied.current
                            ? 'border-indigo-600 text-indigo-600'
                            : 'border-gray-200 text-gray-400 group-hover:border-indigo-600 group-hover:text-indigo-600',
                        'flex h-6 w-6 shrink-0 items-center justify-center rounded-lg border bg-white text-[0.625rem] font-medium',
                        )}
                    >
                        {familienmitglied.initial}
                    </span>
                    <span className="truncate">{familienmitglied.name}</span>
                    </a>
                </li>
                ))}
            </ul>
            </li>
    );
}