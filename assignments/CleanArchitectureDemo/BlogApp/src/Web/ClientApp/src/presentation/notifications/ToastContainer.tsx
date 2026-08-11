// presentation/notifications/ToastContainer.tsx
import { useToastStore } from './toastStore';
import type { ToastVariant } from './toastStore';

const VARIANT_STYLE: Record<ToastVariant, string> = {
  error: 'toast--error',
  warning: 'toast--warning',
  info: 'toast--info',
  success: 'toast--success',
};

export function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts);
  const dismiss = useToastStore((s) => s.dismiss);

  if (toasts.length === 0) return null;

  return (
    <div className="toast-container" role="region" aria-live="polite">
      {toasts.map((toast) => (
        <div key={toast.id} className={`toast ${VARIANT_STYLE[toast.variant]}`} role="alert">
          <div className="toast__content">
            <strong className="toast__title">{toast.title}</strong>
            {toast.detail && <p className="toast__detail">{toast.detail}</p>}
          </div>
          <button
            className="toast__close"
            onClick={() => dismiss(toast.id)}
            aria-label="Đóng thông báo"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
}