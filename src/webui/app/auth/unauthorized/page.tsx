"use client";

import Link from "next/link";

export default function Unauthorized() {

    return (
        <>
        <div className="flex min-h-full flex-col justify-center px-6 py-12 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-sm">
                <img className="mx-auto h-10 w-auto" src="https://tailwindui.com/plus/img/logos/mark.svg?color=indigo&shade=600" alt="Your Company" />
                <h2 className="mt-10 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">Sign in to your account</h2>
            </div>

            <div className="mt-6 sm:mx-auto sm:w-full sm:max-w-sm">
                <form className="space-y-6" method="POST">

                <div>
                    <Link href="/api/auth/signin">
                        <button type="submit" className="flex w-full justify-center rounded-md bg-indigo-600 px-3 py-1.5 xl:text-base sm:text-lg font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600">
                            Anmelden
                        </button>
                    </Link>
                </div>
                </form>

                <p className="mt-6 text-center xl:text-base sm:text-lg text-gray-500">
                    <a href="/auth/register" className="font-semibold leading-6 text-indigo-600 hover:text-indigo-500">Registrieren</a>
                </p>
            </div>
        </div>
              </>
    );
}
