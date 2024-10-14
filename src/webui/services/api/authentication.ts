import axiosInstance from "@/utils/axios";
import { isSuccessHttpStatusCode } from "./base";

export interface LoginRequest {
    login: string,
    email: string,
    password: string | null
}

export interface LoginResponse {
    Id: string,
    username: string,
    email: string,
    token: string
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
    accessToken: string | null | undefined,
    lastName: string | null,
    firstName: string | null
}

export interface GoogleLoginResponse {
    token: string | null
}

export async function Login(request: LoginRequest): Promise<LoginResponse | null> {
    var response = await axiosInstance.post("/auth/login", request);

    if(response.data) {
        var data = response.data as LoginResponse;
        return data;
    }

    return null;
}

export async function Register(request: RegistrationRequest): Promise<boolean> {
    var url = process.env.NEXT_PUBLIC_API_BASE_URL + '/auth/register';
    var response = await fetch(url, {
        body: JSON.stringify(request),
        headers: {
            'Content-Type': 'application/json'
        },
        method: 'POST'
    });

    if(isSuccessHttpStatusCode(response.status)) {
        return true;
    }

    return false;
}

export async function GoogleLogin(request: GoogleLoginRequest): Promise<boolean> {
    var url = process.env.NEXT_PUBLIC_API_BASE_URL + '/auth/google-login';
    var response = await fetch(url, {
        body: JSON.stringify(request),
        headers: {
            'Content-Type': 'application/json'
        },
        method: 'POST'
    });
    
    return isSuccessHttpStatusCode(response.status);
}