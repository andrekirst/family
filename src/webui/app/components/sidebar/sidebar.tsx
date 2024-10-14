import Image from "next/image";
import Navigation from "./navigation";
import FamilyMembers from "./familymembers";
import Settings from "./settings";

export default function Sidebar() {
    return (
        <div className="flex grow flex-col gap-y-5 overflow-y-auto border-r border-gray-200 bg-white px-6 pb-4">
              <div className="flex h-16 shrink-0 items-center">
                <Image
                  alt="Family"
                  src="https://tailwindui.com/plus/img/logos/mark.svg?color=indigo&shade=600"
                  className="h-8 w-auto"
                  width="32"
                  height="32"
                />
              </div>
              <nav className="flex flex-1 flex-col">
                <ul role="list" className="flex flex-1 flex-col gap-y-7">
                  <Navigation />
                  <FamilyMembers />
                  <Settings />
                </ul>
              </nav>
            </div>
    );
}