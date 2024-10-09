'use client';

import { Bars3Icon, XMarkIcon } from "@heroicons/react/24/outline";
//import " ./globals.css";
import "@/app/globals.css"
import { useEffect, useState } from "react";
import { Dialog, DialogBackdrop, DialogPanel, TransitionChild } from "@headlessui/react";
import UserNavigation from "./usernavigation";
import Searchbar from "./searchbar";
import NotificationsBar from "./notificationsbar";
import Sidebar from "./sidebar/sidebar";
import { getProviders, signIn, signOut, useSession } from "next-auth/react";

export default function Home({
    children,
  }: Readonly<{
    children: React.ReactNode;
  }>) {

    const [sidebarOpen, setSidebarOpen] = useState(false);

  const provider: any = null;
  const [authProviders, setAuthProviders] = useState(provider);
  const { data: session } = useSession();
  
  useEffect(() => {
    const setProviders = async() => {
      const response = await getProviders();
      setAuthProviders(response);
    }
    
    setProviders();
  }, []);

  function handleGoogleSignIn(_provider: any) {
    const response = signIn(_provider.id);
  }

  function handleGoogleSignOut() {
    signOut();
  }

    return (
        <>
        {
            session?.user ? (
              <div>
            <Dialog open={sidebarOpen} onClose={setSidebarOpen} className="relative z-50 lg:hidden">
              <DialogBackdrop
                transition
                className="fixed inset-0 bg-gray-900/80 transition-opacity duration-300 ease-linear data-[closed]:opacity-0"
              />
  
              <div className="fixed inset-0 flex">
                <DialogPanel
                  transition
                  className="relative mr-16 flex w-full max-w-xs flex-1 transform transition duration-300 ease-in-out data-[closed]:-translate-x-full"
                >
                  <TransitionChild>
                    <div className="absolute left-full top-0 flex w-16 justify-center pt-5 duration-300 ease-in-out data-[closed]:opacity-0">
                      <button type="button" onClick={() => setSidebarOpen(false)} className="-m-2.5 p-2.5">
                        <span className="sr-only">Close sidebar</span>
                        <XMarkIcon aria-hidden="true" className="h-6 w-6 text-white" />
                      </button>
                    </div>
                  </TransitionChild>
                  
                  <Sidebar />
  
                </DialogPanel>
              </div>
            </Dialog>
  
            <div className="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-72 lg:flex-col">
              <Sidebar />
            </div>
  
            <div className="lg:pl-72">
              <div className="sticky top-0 z-40 flex h-16 shrink-0 items-center gap-x-4 border-b border-gray-200 bg-white px-4 shadow-sm sm:gap-x-6 sm:px-6 lg:px-8">
                <button type="button" onClick={() => setSidebarOpen(true)} className="-m-2.5 p-2.5 text-gray-700 lg:hidden">
                  <span className="sr-only">Open sidebar</span>
                  <Bars3Icon aria-hidden="true" className="h-6 w-6" />
                </button>
  
                <div aria-hidden="true" className="h-6 w-px bg-gray-200 lg:hidden" />
  
                <div className="flex flex-1 gap-x-4 self-stretch lg:gap-x-6">
                  <Searchbar />
                  <div className="flex items-center gap-x-4 lg:gap-x-6">
                    <NotificationsBar />
  
                    <div aria-hidden="true" className="hidden lg:block lg:h-6 lg:w-px lg:bg-gray-200" />
  
                    <UserNavigation />
                  </div>
                </div>
              </div>
  
              <main className="py-10">
                <div className="px-4 sm:px-6 lg:px-8">
                  {children}
                </div>
              </main>
            </div>
          </div>
            ) : (
              (authProviders && Object.values(authProviders).map((provider: any) => (
                <button onClick={() => handleGoogleSignIn(provider)} type='button' key={provider.name}>Sign In With {provider.name}</button>
              )))
            )
          }
          </>
    )
}