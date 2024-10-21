import { get, isSuccessHttpStatusCode, post } from "./base"

export interface getCalendarMeListData
{
    count: number,
    items: {
        id: string,
        name: string
    }[]
}

export async function getCalendarMeList(token: string): Promise<getCalendarMeListData> {
    return await get<getCalendarMeListData>('/calendar/me/list', token);
}

export async function createCalendar(token: string, body: BodyInit | null | undefined): Promise<boolean> {
    var response = await post('/calendar', body, token);
    return isSuccessHttpStatusCode(response.status);
}