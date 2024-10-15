export function isSuccessHttpStatusCode(httpStatusCode: number) : boolean {
    return httpStatusCode >= 200 && httpStatusCode <= 299;
}

function buildUrl(endpoint: string): string {
    return process.env.NEXT_PUBLIC_API_BASE_URL + endpoint;
}

function secureHeader(token: string): HeadersInit | undefined {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

export async function get<TResult>(endpoint: string, token: string): Promise<TResult> {
    const url = buildUrl(endpoint);
    const response = fetch(
        url,
        {
            headers: secureHeader(token),
            method: 'GET'
        }
    );

    return response
        .then((r) => r.json())
        .then((data: TResult) => data);
}

export async function post(endpoint: string, body: BodyInit | null | undefined, token: string): Promise<Response> {
    const url = buildUrl(endpoint);
    const response = await fetch(
        url,
        {
            body: body,
            headers: secureHeader(token),
            method: 'POST'
        }
    )

    return response;
}

export async function postAnonymous(endpoint: string, body: BodyInit | null | undefined): Promise<Response> {
    const url = buildUrl(endpoint);
    const response = await fetch(
        url,
        {
            body: body,
            headers: {
                'Content-Type': 'application/json'
            },
            method: 'POST'
        }
    )

    return response;
}