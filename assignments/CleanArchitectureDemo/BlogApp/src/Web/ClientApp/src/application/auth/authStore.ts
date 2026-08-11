// application/auth/authStore.ts
import { create } from 'zustand';
import type { IAuthApi, RegisterPayload } from './ports';
import type { AuthSession } from '../../domain/auth/types';

interface AuthState {
  session: AuthSession | null;
  isLoading: boolean;
  error: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => Promise<void>;
  logoutAll: () => Promise<void>;
  setSession: (session: AuthSession) => void;
}

const STORAGE_KEY = 'blogapp.auth';

function persist(session: AuthSession | null) {
  if (session) localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  else localStorage.removeItem(STORAGE_KEY);
}

// Đọc đồng bộ ngay khi module load — chạy trước khi React render bất cứ thứ gì,
// nên không còn khoảng trống thời gian để ProtectedRoute thấy session = null.
function readStoredSession(): AuthSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as AuthSession) : null;
  } catch {
    return null; // localStorage bị corrupt hoặc bị block (private mode) -> coi như chưa đăng nhập
  }
}

export function createAuthStore(authApi: IAuthApi) {
  return create<AuthState>((set, get) => ({
    session: readStoredSession(), // <-- khởi tạo ngay, không phải null nữa
    isLoading: false,
    error: null,

    setSession: (session) => {
      persist(session);
      set({ session });
    },

    login: async (email, password) => {
      set({ isLoading: true, error: null });
      try {
        const session = await authApi.login(email, password);
        get().setSession(session);
        set({ isLoading: false });
      } catch {
        set({ isLoading: false, error: 'Email hoặc mật khẩu không đúng.' });
        throw new Error('Login failed');
      }
    },

    register: async (payload) => {
      set({ isLoading: true, error: null });
      try {
        await authApi.register(payload);
        set({ isLoading: false });
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Đăng ký thất bại, vui lòng thử lại.';
        set({ isLoading: false, error: message });
        throw err;
      }
    },

    logout: async () => {
      const { session } = get();
      if (session) {
        try {
          await authApi.logout(session.refreshToken);
        } catch {
          // Vẫn clear local state dù API lỗi -> tránh kẹt UX
        }
      }
      persist(null);
      set({ session: null });
    },

    logoutAll: async () => {
      try {
        await authApi.logoutAll();
      } finally {
        persist(null);
        set({ session: null });
      }
    },
  }));
}