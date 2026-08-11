// presentation/pages/public/BlogListPage.tsx
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { postApi } from '../../../infrastructure/posts/postApi';
import { PublicLayout } from '../../layouts/PublicLayout';
import type { PostSummary } from '../../../domain/posts/types';

export function BlogListPage() {
  const [posts, setPosts] = useState<PostSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        // Không truyền authorId -> handler tự lọc chỉ bài IsPublished cho khách vãng lai
        const data = await postApi.getPosts();
        if (!cancelled) setPosts(data);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <PublicLayout>
      <div className="blog-hero">
        <p className="dash-page__eyebrow">Trang bìa</p>
        <h1 className="blog-hero__title">Những gì đang được viết</h1>
        <p className="blog-hero__tagline">Bài viết mới nhất từ các tác giả trên BlogApp.</p>
      </div>

      {isLoading ? (
        <p className="dash-page__status">Đang tải bài viết…</p>
      ) : posts.length === 0 ? (
        <p className="dash-page__status">Chưa có bài viết nào được xuất bản.</p>
      ) : (
        <ul className="blog-grid">
          {posts.map((post) => (
            <li key={post.id} className="blog-card">
              <Link to={`/blog/${post.id}`} className="blog-card__link">
                <h2 className="blog-card__title">{post.title}</h2>
                <p className="blog-card__excerpt">
                  {post.content.slice(0, 160)}
                  {post.content.length > 160 ? '…' : ''}
                </p>
                <div className="blog-card__meta">
                  <span>{post.authorFullName}</span>
                  {post.publishedDate && (
                    <span>{new Date(post.publishedDate).toLocaleDateString('vi-VN')}</span>
                  )}
                  <span>{post.commentsCount} bình luận</span>
                </div>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </PublicLayout>
  );
}