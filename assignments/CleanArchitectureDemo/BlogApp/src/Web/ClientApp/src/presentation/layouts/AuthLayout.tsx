// presentation/layouts/AuthLayout.tsx
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

interface AuthLayoutProps {
  eyebrow: string;
  title: string;
  tagline: string;
  children: ReactNode;
  footer?: ReactNode;
}

export function AuthLayout({ eyebrow, title, tagline, children, footer }: AuthLayoutProps) {
  return (
    <div className="auth-shell">
      <aside className="auth-shell__panel">
        <div className="auth-shell__mark">
          <svg viewBox="0 0 24 24" width="26" height="26" fill="none" aria-hidden="true">
            <circle cx="12" cy="12" r="11" stroke="var(--brass)" strokeWidth="1.4" />
            <path
              d="M8 12.5l2.6 2.6L16.5 9"
              stroke="var(--brass)"
              strokeWidth="1.6"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
          <span className="auth-shell__wordmark">BlogApp</span>
        </div>

        <div className="auth-shell__copy">
          <p className="auth-shell__eyebrow">{eyebrow}</p>
          <h1 className="auth-shell__title">{title}</h1>
          <p className="auth-shell__tagline">{tagline}</p>
        </div>

        <p className="auth-shell__footnote">Viết. Xuất bản. Được đọc.</p>
        <Link to="/blog" className="auth-shell__browse-link">Xem bài viết công khai →</Link>
      </aside>

      <main className="auth-shell__card-wrap">
        <div className="auth-shell__card">{children}</div>
        {footer && <div className="auth-shell__card-footer">{footer}</div>}
      </main>
    </div>
  );
}