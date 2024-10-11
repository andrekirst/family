import axiosInstance from "@/utils/axios";

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

export interface GoogleLoginRequest {
    email: string,
    name: string,
    googleId: string
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

export async function GoogleLogin(request: GoogleLoginRequest): Promise<GoogleLoginResponse | null> {
    var response = await axiosInstance.post("/auth/google-login", request);

    if(response.data) {
        var data = response.data as GoogleLoginResponse;
        return data;
    }

    return null;
}