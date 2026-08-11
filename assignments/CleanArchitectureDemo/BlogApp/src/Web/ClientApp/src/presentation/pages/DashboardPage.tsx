// presentation/pages/DashboardPage.tsx
import { useAuthStore } from '../../composition';
import { DashboardLayout } from '../layouts/DashboardLayout';
import { AdminDashboard } from './dashboard/AdminDashboard';
import { AuthorDashboard } from './dashboard/AuthorDashboard';

export function DashboardPage() {
  const session = useAuthStore((s) => s.session);

  // ProtectedRoute đã chặn user chưa login trước khi vào đây,
  // check null này chỉ để type-safety cho TypeScript.
  if (!session) return null;

  return (
    <DashboardLayout>
      {session.role === 'Admin' ? <AdminDashboard /> : <AuthorDashboard />}
    </DashboardLayout>
  );
}