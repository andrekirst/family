// User Types
export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  preferredLanguage?: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

// Authentication Input Types
export interface LoginInput {
  email: string;
  password: string;
}

export interface LoginCallbackInput {
  authorizationCode: string;
  state: string;
}

export interface RefreshTokenInput {
  refreshToken: string;
}

// Authentication Response Types
export interface LoginPayload {
  accessToken?: string;
  refreshToken?: string;
  user?: User;
  errors?: string[];
}

export interface LoginInitiationPayload {
  loginUrl: string;
  state: string;
}

export interface RefreshTokenPayload {
  accessToken?: string;
  refreshToken?: string;
  errors?: string[];
}

export interface LogoutPayload {
  success: boolean;
  errors?: string[];
}

// GraphQL Response Wrappers
export interface DirectLoginResponse {
  directLogin: LoginPayload;
}

export interface InitiateLoginResponse {
  initiateLogin: LoginInitiationPayload;
}

export interface CompleteLoginResponse {
  completeLogin: LoginPayload;
}

export interface RefreshTokenResponse {
  refreshToken: RefreshTokenPayload;
}

export interface LogoutResponse {
  logout: LogoutPayload;
}

export interface GetCurrentUserResponse {
  currentUser: User;
}

export interface GetUserByIdResponse {
  userById: User;
}

export interface GetAllUsersResponse {
  users: User[];
}