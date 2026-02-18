export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
}

export interface UserInfo {
  email: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
}

export interface User extends UserInfo {
  isAuthenticated: boolean;
  isAdmin: boolean;
}
