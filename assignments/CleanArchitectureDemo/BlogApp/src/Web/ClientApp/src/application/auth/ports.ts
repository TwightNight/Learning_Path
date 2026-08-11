// application/auth/ports.ts
import type { AuthSession } from '../../domain/auth/types';

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
}

export interface RegisterResult {
  userId: number;
  userName: string;
  email: string;
}

export interface IAuthApi {
  login(email: string, password: string): Promise<AuthSession>;
  register(payload: RegisterPayload): Promise<RegisterResult>;
  logout(refreshToken: string): Promise<void>;
  logoutAll(): Promise<void>;
  refresh(refreshToken: string): Promise<AuthSession>;
}