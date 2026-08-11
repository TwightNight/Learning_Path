// composition.ts
import { authApi } from './infrastructure/auth/authApi';
import { createAuthStore } from './application/auth/authStore';
import { attachAuthInterceptor, attachErrorToastInterceptor } from './infrastructure/http/httpClient';
import { useToastStore } from './presentation/notifications/toastStore';



export const useAuthStore = createAuthStore(authApi);

attachAuthInterceptor({
  getAccessToken: () => useAuthStore.getState().session?.accessToken,
  getRefreshToken: () => useAuthStore.getState().session?.refreshToken,
  refresh: (refreshToken) => authApi.refresh(refreshToken),
  onRefreshSuccess: (session) => useAuthStore.getState().setSession(session),
  onRefreshFailure: () => useAuthStore.getState().logout(),
});

attachErrorToastInterceptor({
  showToast: (appError) =>
    useToastStore.getState().show({
      variant: appError.status === 0 ? 'warning' : 'error',
      title: appError.title,
      detail: appError.detail,
    }),
});