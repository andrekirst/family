import { get } from "./base";

export interface getFamilyMembersData
{
    id: string,
    displayText: string,
    href: string,
    current: boolean
}

export async function getFamilyMembers(token: string): Promise<getFamilyMembersData[]> {
    return await get<getFamilyMembersData[]>('/frontend/webui/sidebar/familymembers', token)
        .then((items) => {
            items.forEach((item) => {
                item.href = `/familymember/${item.id}/start`
            });
            
            return items;
        });
}