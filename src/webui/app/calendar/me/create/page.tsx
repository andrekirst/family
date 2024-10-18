"use client";

import { toJson } from "@/services/api/base";
import { createCalendar } from "@/services/api/calendar";
import { useSession } from "next-auth/react";
import { FormEvent } from "react";

export default function Page() {
    const { data: session } = useSession();    

    async function onSubmit(event: FormEvent<HTMLFormElement>) {
        // @ts-ignore
        const token = session?.user.accessToken;

        event.preventDefault();
        const formData = new FormData(event.currentTarget);
        console.log(formData);
        console.log(JSON.stringify(formData));
        console.log(toJson(formData))
        const json = toJson(formData);
        const response = await createCalendar(token, json);
        if(response == true) {
            
        }
    }

    return(
        <>
            <form onSubmit={onSubmit}>
                <div className="space-y-12">
                    <div className="border-b border-gray-900/10 pb-12">
                        <h2 className="text-base font-semibold leading-7 text-gray-900">Kalender</h2>

                        <div className="mt-10 grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                            <div className="sm:col-span-4">
                                <label htmlFor="name" className="block text-sm font-medium leading-6 text-gray-900">
                                    Name
                                </label>
                                <div className="mt-2">
                                    <input
                                    id="name"
                                    name="name"
                                    type="text"
                                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex items-center justify-start gap-x-6">
                    <button
                        type="submit"
                        className="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                        >
                        Speichern
                    </button>
                    <button type="button" className="rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50">
                        Abbrechen
                    </button>
                    
                </div>
            </form>
        </>
    );
}