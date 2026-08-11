// infrastructure/auth/authApi.ts
import { httpClient } from '../http/httpClient';
import type { IAuthApi, RegisterPayload, RegisterResult } from '../../application/auth/ports';
import type { AuthSession } from '../../domain/auth/types';

interface LoginResponseDto {
  accessToken: string;
  accessTokenExpiry: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  userId: number;
  role: string;
}

function toSession(dto: LoginResponseDto): AuthSession {
  return { ...dto };
}

export const authApi: IAuthApi = {
  async login(email, password) {
    const { data } = await httpClient.post<LoginResponseDto>(
      '/auth/login', 
      { email, password },
      {suppressToast: true}
    );
    return toSession(data);
  },
  async register(payload: RegisterPayload) {
    const { data } = await httpClient.post<RegisterResult>('/auth/register', 
      {
        firstName: payload.firstName,
        lastName: payload.lastName,
        userName: payload.userName,
        email: payload.email,
        password: payload.password,
      },
      { suppressToast: true },
    );
    return data;
  },
  async logout(refreshToken) {
    await httpClient.post('/auth/logout', { refreshToken });
  },
  async logoutAll() {
    await httpClient.post('/auth/logout-all');
  },
  async refresh(refreshToken) {
    const { data } = await httpClient.post<LoginResponseDto>('/auth/refresh', { refreshToken });
    return toSession(data);
  },
};