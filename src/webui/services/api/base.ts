export function isSuccessHttpStatusCode(httpStatusCode: number) : boolean {
    return httpStatusCode >= 200 && httpStatusCode <= 299;
}