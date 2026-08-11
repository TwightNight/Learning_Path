// presentation/pages/dashboard/AuthorDashboard.tsx
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { postApi } from '../../../infrastructure/posts/postApi';
import { useAuthStore } from '../../../composition';
import type { PostSummary } from '../../../domain/posts/types';

export function AuthorDashboard() {
  const userId = useAuthStore((s) => s.session?.userId);
  const [posts, setPosts] = useState<PostSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<number | null>(null);

  async function loadPosts() {
    if (!userId) return;
    setIsLoading(true);
    try {
      const data = await postApi.getPosts(userId);
      setPosts(data);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadPosts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  async function handleTogglePublish(post: PostSummary) {
    const action = post.isPublished ? postApi.unpublish : postApi.publish;
    try {
      await action(post.id);
      setPosts((prev) =>
        prev.map((p) => (p.id === post.id ? { ...p, isPublished: !p.isPublished } : p)),
      );
    } catch {
      // Lỗi đã hiện qua toast interceptor toàn cục.
    }
  }

  async function handleDelete(id: number) {
    if (confirmingDeleteId !== id) {
      setConfirmingDeleteId(id); // Bước 1: yêu cầu bấm lần 2 để xác nhận
      return;
    }
    try {
      await postApi.remove(id);
      setPosts((prev) => prev.filter((p) => p.id !== id));
    } finally {
      setConfirmingDeleteId(null);
    }
  }

  const publishedCount = posts.filter((p) => p.isPublished).length;
  const draftCount = posts.length - publishedCount;
  const commentsTotal = posts.reduce((sum, p) => sum + p.commentsCount, 0);

  const stats = [
    { label: 'Đã xuất bản', value: publishedCount },
    { label: 'Bản nháp', value: draftCount },
    { label: 'Tổng bình luận', value: commentsTotal },
  ];

  if (isLoading) {
    return <p className="dash-page__status">Đang tải bài viết…</p>;
  }

  return (
    <div className="dash-page">
      <div className="dash-page__head-row">
        <div>
          <p className="dash-page__eyebrow">Bàn viết của bạn</p>
          <h1 className="dash-page__title">Chào, tác giả</h1>
        </div>
        <Link to="/posts/new" className="dash-page__cta">Viết bài mới</Link>
      </div>

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
          <h2>Bài viết của bạn</h2>
        </div>

        {posts.length === 0 ? (
          <p className="dash-page__status">Bạn chưa có bài viết nào — bắt đầu viết bài đầu tiên.</p>
        ) : (
          <ul className="dash-postlist">
            {posts.map((post) => (
              <li key={post.id} className="dash-postlist__item">
                <div>
                  <p className="dash-postlist__title">{post.title}</p>
                  <p className="dash-postlist__meta">
                    {post.commentsCount} bình luận
                    {post.publishedDate &&
                      ` · Xuất bản ${new Date(post.publishedDate).toLocaleDateString('vi-VN')}`}
                  </p>
                </div>

                <div className="dash-postlist__actions">
                  <span className={`dash-tag ${post.isPublished ? 'dash-tag--active' : 'dash-tag--draft'}`}>
                    {post.isPublished ? 'Đã xuất bản' : 'Bản nháp'}
                  </span>
                  <button className="dash-table__action" onClick={() => handleTogglePublish(post)}>
                    {post.isPublished ? 'Gỡ xuất bản' : 'Xuất bản'}
                  </button>
                  <Link to={`/posts/${post.id}/edit`} className="dash-table__action">
                    Sửa
                  </Link>
                  <button
                    className="dash-table__action dash-table__action--danger"
                    onClick={() => handleDelete(post.id)}
                  >
                    {confirmingDeleteId === post.id ? 'Bấm lần nữa để xoá' : 'Xoá'}
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}