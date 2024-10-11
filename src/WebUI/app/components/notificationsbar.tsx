import { BellIcon } from "@heroicons/react/24/outline";

export default function NotificationsBar() {
    return (
        <div className="flex items-center gap-x-4 lg:gap-x-6">
            <button type="button" className="-m-2.5 p-2.5 text-gray-400 hover:text-gray-500">
            <span className="sr-only">View notifications</span>
            <BellIcon aria-hidden="true" className="h-6 w-6" />
            </button>

            
        </div>
    );
}