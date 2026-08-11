// presentation/layouts/DashboardLayout.tsx
import type { ReactNode } from 'react';
import { useAuthStore } from '../../composition';
import { RoleBadge } from '../components/RoleBadge';

export function DashboardLayout({ children }: { children: ReactNode }) {
  const session = useAuthStore((s) => s.session);
  const logout = useAuthStore((s) => s.logout);
  const logoutAll = useAuthStore((s) => s.logoutAll);

  return (
    <div className="dash-shell">
      <header className="dash-shell__header">
        <span className="dash-shell__wordmark">BlogApp</span>

        <div className="dash-shell__account">
          {session && <RoleBadge role={session.role} size={30} />}
          <div className="dash-shell__account-actions">
            <button className="dash-shell__link" onClick={() => logout()}>Đăng xuất</button>
            <button className="dash-shell__link" onClick={() => logoutAll()}>Đăng xuất mọi thiết bị</button>
          </div>
        </div>
      </header>

      <main className="dash-shell__content">{children}</main>
    </div>
  );
}