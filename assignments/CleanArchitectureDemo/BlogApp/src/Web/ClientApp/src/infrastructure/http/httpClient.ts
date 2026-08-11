// infrastructure/http/httpClient.ts
import axios, { type InternalAxiosRequestConfig } from 'axios';
import type { AuthSession } from '../../domain/auth/types';
import { parseApiError } from '../errors/parseApiError';
import type { AppError } from '../../domain/errors/problemDetails';

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
});

// _retry: đánh dấu request đã từng retry để tránh loop vô hạn nếu refresh xong vẫn 401
interface RetriableConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let isRefreshing = false;
let refreshSubscribers: ((accessToken: string) => void)[] = [];

function subscribeTokenRefresh(cb: (accessToken: string) => void) {
  refreshSubscribers.push(cb);
}

function notifySubscribers(accessToken: string) {
  refreshSubscribers.forEach((cb) => cb(accessToken));
  refreshSubscribers = [];
}

function rejectSubscribers() {
  refreshSubscribers = [];
}

interface AttachAuthDeps {
  getAccessToken: () => string | undefined;
  getRefreshToken: () => string | undefined;
  refresh: (refreshToken: string) => Promise<AuthSession>;
  onRefreshSuccess: (session: AuthSession) => void;
  onRefreshFailure: () => void;
}

export function attachAuthInterceptor(deps: AttachAuthDeps) {
  httpClient.interceptors.request.use((config) => {
    const token = deps.getAccessToken();
    if (token) {
      config.headers = config.headers ?? {};
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  });

  httpClient.interceptors.response.use(
    (res) => res,
    async (error) => {
      const originalRequest = error.config as RetriableConfig | undefined;

      if (!originalRequest || error.response?.status !== 401 || originalRequest._retry) {
        return Promise.reject(error);
      }

      // 401 đến từ chính /auth/refresh hoặc /auth/login -> không refresh, logout luôn
      const url = originalRequest.url ?? '';
      if (url.includes('/auth/refresh') || url.includes('/auth/login')) {
        deps.onRefreshFailure();
        return Promise.reject(error);
      }

      const refreshToken = deps.getRefreshToken();
      if (!refreshToken) {
        deps.onRefreshFailure();
        return Promise.reject(error);
      }

      originalRequest._retry = true;

      // Nếu đang refresh (do request khác trigger), xếp hàng chờ token mới
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          subscribeTokenRefresh((newAccessToken) => {
            originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
            httpClient(originalRequest).then(resolve).catch(reject);
          });
        });
      }

      isRefreshing = true;
      try {
        const session = await deps.refresh(refreshToken);
        deps.onRefreshSuccess(session);
        notifySubscribers(session.accessToken);

        originalRequest.headers.Authorization = `Bearer ${session.accessToken}`;
        return httpClient(originalRequest);
      } catch (refreshError) {
        rejectSubscribers();
        deps.onRefreshFailure();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    },
  );
}

declare module 'axios' {
  export interface AxiosRequestConfig {
    suppressToast?: boolean;
  }
}

interface AttachErrorToastDeps {
  showToast: (error: AppError) => void;
}

export function attachErrorToastInterceptor(deps: AttachErrorToastDeps) {
  httpClient.interceptors.response.use(
    (res) => res,
    (error) => {
      // error.config có thể undefined nếu lỗi xảy ra trước khi request được gửi
      if (!error.config?.suppressToast) {
        const appError = parseApiError(error);
        // Field-level errors (400 validation) thường đã hiển thị ngay tại form -> chỉ toast phần chung
        deps.showToast(appError);
      }
      return Promise.reject(error);
    },
  );
}