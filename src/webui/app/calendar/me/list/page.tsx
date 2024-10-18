"use client";

import { useEffect, useState } from "react";
import Empty from "./empty";
import { getCalendarMeList, getCalendarMeListData } from "@/services/api/calendar";
import { useSession } from "next-auth/react";
import List from "./list";

export default function Page() {
    const [calendars, setCalendars] = useState<getCalendarMeListData>({ count: 0, items: [] });

    const { data: session } = useSession();

    useEffect(() => {
        const fetchData = async () => {
            // @ts-ignore
            const token = session?.user.accessToken;
            const result = await getCalendarMeList(token);
            setCalendars(result);
        };

            fetchData();
    }, []);

    return(
        <>
            {
                calendars?.count == 0
                    ? <Empty />
                    : <List items={calendars?.items} />
            }
            
        </>
    );
}