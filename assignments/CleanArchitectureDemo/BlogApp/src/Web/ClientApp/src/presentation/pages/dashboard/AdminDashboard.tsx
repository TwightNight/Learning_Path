import { useEffect, useState } from 'react';
import { userApi } from '../../../infrastructure/users/userApi';
import { postApi } from '../../../infrastructure/posts/postApi';
import type { UserSummary } from '../../../domain/users/types';
import type { PostSummary } from '../../../domain/posts/types';

export function AdminDashboard() {
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [posts, setPosts] = useState<PostSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      try {
        const [usersData, postsData] = await Promise.all([
          userApi.getUsers(),
          postApi.getAllPostsForAdmin(),
        ]);
        if (!cancelled) {
          setUsers(usersData);
          setPosts(postsData);
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  async function toggleActive(user: UserSummary) {
    const action = user.isActive ? userApi.deactivate : userApi.activate;
    try {
      await action(user.id);
      setUsers((prev) => prev.map((u) => (u.id === user.id ? { ...u, isActive: !u.isActive } : u)));
    } catch {
      // Lỗi đã hiện qua toast interceptor toàn cục, không cần xử lý thêm ở đây.
    }
  }

  const draftCount = posts.filter((p) => !p.isPublished).length;

  const stats = [
    { label: 'Tổng bài viết', value: posts.length },
    { label: 'Người dùng', value: users.length },
    { label: 'Bản nháp toàn hệ thống', value: draftCount },
  ];

  if (isLoading) {
    return <p className="dash-page__status">Đang tải dữ liệu…</p>;
  }

  return (
    <div className="dash-page">
      <p className="dash-page__eyebrow">Bảng điều khiển quản trị</p>
      <h1 className="dash-page__title">Toàn cảnh hệ thống</h1>

      <div className="dash-stats">
        {stats.map((stat) => (
          <div key={stat.label} className="dash-stats__card">
            <span className="dash-stats__value">{stat.value}</span>
            <span className="dash-stats__label">{stat.label}</span>
          </div>
        ))}
      </div>

      <section className="dash-panel">
        <div className="dash-panel__header">
          <h2>Người dùng</h2>
          <span className="dash-panel__hint">{users.length} tài khoản</span>
        </div>

        {users.length === 0 ? (
          <p className="dash-page__status">Chưa có người dùng nào.</p>
        ) : (
          <table className="dash-table">
            <thead>
              <tr>
                <th>Tên</th>
                <th>Vai trò</th>
                <th>Trạng thái</th>
                <th aria-label="Hành động" />
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.fullName}</td>
                  <td>{user.role}</td>
                  <td>
                    <span className={`dash-tag ${user.isActive ? 'dash-tag--active' : 'dash-tag--inactive'}`}>
                      {user.isActive ? 'Đang hoạt động' : 'Đã khoá'}
                    </span>
                  </td>
                  <td className="dash-table__actions">
                    <button className="dash-table__action" onClick={() => toggleActive(user)}>
                      {user.isActive ? 'Khoá' : 'Mở lại'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </div>
  );
}