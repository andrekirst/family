import axiosInstance from "@/utils/axios";
import { isSuccessHttpStatusCode, postAnonymous } from "./base";

export interface LoginRequest {
    login: string,
    password: string
}

export interface LoginResponse {
    Id: string,
    username: string,
    email: string,
    token: string,
    name: string
}

export interface RegistrationRequest {
    firstName: string,
    lastName: string,
    birthdate: string,
    email: string,
    username: string,
    password: string
}

export interface GoogleLoginRequest {
    email: string,
    name: string,
    googleId: string,
    googleAccessToken: string | null | undefined,
    accessToken: string | null | undefined,
    lastName: string | null,
    firstName: string | null
}

export interface GoogleLoginResponse {
    token: string | null
}

export async function Login(request: LoginRequest): Promise<LoginResponse | null> {
    const response = await postAnonymous(
        '/auth/login',
        JSON.stringify(request));

    const json = await response.json();

    if(json) {
        return json as LoginResponse;
    }

    return null;
}

export async function Register(request: RegistrationRequest): Promise<boolean> {
    const response = await postAnonymous(
        '/auth/register',
        JSON.stringify(request));

    if(isSuccessHttpStatusCode(response.status)) {
        return true;
    }

    return false;
}

export async function GoogleLogin(request: GoogleLoginRequest): Promise<LoginResponse | null> {
    const response = await postAnonymous(
        '/auth/google-login',
        JSON.stringify(request)
    )
    
    const success = isSuccessHttpStatusCode(response.status);

    if(!success) { return null; }

    var value = await response.json().then((data: LoginResponse) => data);
    return value;
}