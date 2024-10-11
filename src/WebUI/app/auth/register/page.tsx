"use client";

import { redirect } from "next/navigation";
import { useState } from "react";

export default function Register() {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    username: '',
    birthdate: new Date()
  });

  const handleChange = (e) => {
    const { firstName, lastName, email, password, username, birthdate } = e.target;
    setFormData((pervData) => ({
      ...prevData,
      [firstName]: firstName,
      [lastName]: lastName,
      [email]: email,
      [password]: password,
      [username]: username,
      [birthdate]: birthdate
    }))
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await fetch(process.env.API_BASE_URL + '/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });

      if(response.ok) {
        redirect("/");
      }
      else{
        alert('asd');
      }
    } catch (error) {
      
    }
  };

    return (
        <>
            <div className="flex min-h-full justify-center px-6 py-12 lg:px-8 space-y-6 sm:px-6 lg:col-span-9">
              <section aria-labelledby="payment-details-heading">
                <form onSubmit={handleSubmit}>
                  <div className="shadow sm:overflow-hidden sm:rounded-md">
                    <div className="bg-white px-4 py-6 sm:p-6">
                      <div>
                        <h2 id="payment-details-heading" className="text-lg font-medium leading-6 text-gray-900">
                          Neues Familienmitglied
                        </h2>
                      </div>

                      <div className="mt-6 grid grid-cols-4 gap-6">
                        <div className="col-span-4 sm:col-span-2">
                          <label htmlFor="firstName" className="block text-sm font-medium leading-6 text-gray-900">
                            Vorname
                          </label>
                          <input
                            id="firstName"
                            name="firstName"
                            type="text"
                            autoComplete="cc-given-name"
                            className="mt-2 block w-full rounded-md border-0 px-3 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-gray-900 sm:text-sm sm:leading-6"
                          />
                        </div>

                        <div className="col-span-4 sm:col-span-2">
                          <label htmlFor="lastName" className="block text-sm font-medium leading-6 text-gray-900">
                            Nachname
                          </label>
                          <input
                            id="lastName"
                            name="lastName"
                            type="text"
                            autoComplete="cc-family-name"
                            className="mt-2 block w-full rounded-md border-0 px-3 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-gray-900 sm:text-sm sm:leading-6"
                          />
                        </div>

                        <div className="col-span-4 sm:col-span-2">
                          <label htmlFor="email" className="block text-sm font-medium leading-6 text-gray-900">
                            EMail-Adresse
                          </label>
                          <input
                            id="email"
                            name="email"
                            type="email"
                            autoComplete="email"
                            className="mt-2 block w-full rounded-md border-0 px-3 py-1.5 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-gray-900 sm:text-sm sm:leading-6"
                          />
                        </div>

                        <div className="col-span-4 sm:col-span-2">
                          <label htmlFor="password" className="block text-sm font-medium leading-6 text-gray-900">
                            Passwort
                          </label>
                          <input
                            id="password"
                            name="password"
                            type="password"
                            autoComplete="new-password"
                            className="mt-2 block w-full rounded-md border-0 px-3 py-1.5 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-gray-900 sm:text-sm sm:leading-6"
                          />
                        </div>

                        <div className="col-span-4 sm:col-span-2">
                          <label htmlFor="username" className="block text-sm font-medium leading-6 text-gray-900">
                            Benutzername
                          </label>
                          <input
                            id="username"
                            name="username"
                            type="text"
                            autoComplete="username"
                            className="mt-2 block w-full rounded-md border-0 px-3 py-1.5 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-gray-900 sm:text-sm sm:leading-6"
                          />
                        </div>

                        <div className="col-span-4 sm:col-span-2">
                          <label htmlFor="birthdate" className="block text-sm font-medium leading-6 text-gray-900">
                            Geburtstag
                          </label>
                          <input
                            id="birthdate"
                            name="birthdate"
                            type="date"
                            autoComplete="bday-day"
                            className="mt-2 block w-full rounded-md border-0 px-3 py-1.5 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-gray-900 sm:text-sm sm:leading-6"
                          />
                        </div>
                      </div>
                    </div>
                    <div className="bg-gray-50 px-4 py-3 text-right sm:px-6">
                      <button
                        type="submit"
                        className="inline-flex justify-center rounded-md bg-gray-900 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-gray-700 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900"
                      >
                        Register
                      </button>
                    </div>
                  </div>
                </form>
              </section>
              </div>
        </>
    );
}