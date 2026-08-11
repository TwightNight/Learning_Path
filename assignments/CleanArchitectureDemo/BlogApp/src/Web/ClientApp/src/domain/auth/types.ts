// domain/auth/types.ts
export interface AuthSession {
  accessToken: string;
  accessTokenExpiry: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  userId: number;
  role: string;
}