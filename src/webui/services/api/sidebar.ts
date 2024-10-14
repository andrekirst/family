export interface getFamilyMembersData
{
    id: string,
    displayText: string,
    href: string,
    current: boolean
}

export async function getFamilyMembers(token: string): Promise<getFamilyMembersData[]> {
    const url = process.env.NEXT_PUBLIC_API_BASE_URL + '/frontend/webui/sidebar/familymembers';

    const response = await fetch(url, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        method: 'GET'
    })
    .then((response) => {
        return response.json();
    })
    .then((data: getFamilyMembersData[]) => {
        return data;
    })
    .then((items) => {
        items.forEach((item) => {
            item.href = `/familymember/${item.id}/start`
        });
        
        return items;
    });
    console.log(response);
    return response;
}