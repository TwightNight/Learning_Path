// presentation/layouts/PublicLayout.tsx
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

export function PublicLayout({ children }: { children: ReactNode }) {
  return (
    <div className="public-shell">
      <header className="public-shell__header">
        <Link to="/blog" className="public-shell__wordmark">BlogApp</Link>
        <nav className="public-shell__nav">
          <Link to="/login" className="public-shell__link">Đăng nhập</Link>
          <Link to="/register" className="public-shell__cta">Viết trên BlogApp</Link>
        </nav>
      </header>
      <main className="public-shell__content">{children}</main>
    </div>
  );
}