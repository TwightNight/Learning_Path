// presentation/pages/public/BlogPostPage.tsx
import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { httpClient } from '../../../infrastructure/http/httpClient';
import { PublicLayout } from '../../layouts/PublicLayout';

interface AuthorDto {
  id: number;
  fullName: string | null;
}

interface CommentDto {
  id: number;
  content: string | null;
  userId: number;
  userFullName: string | null;
}

interface PostDetailFull {
  id: number;
  title: string | null;
  content: string | null;
  publishedDate: string | null;
  isPublished: boolean;
  author: AuthorDto | null;
  comments: CommentDto[] | null;
}

export function BlogPostPage() {
  const { id } = useParams();
  const [post, setPost] = useState<PostDetailFull | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        // suppressToast: 404 ở đây (bài không tồn tại/chưa publish) đã có UI riêng, không cần toast
        const { data } = await httpClient.get<PostDetailFull>(`/posts/${id}`, { suppressToast: true });
        if (!cancelled) setPost(data);
      } catch {
        if (!cancelled) setNotFound(true);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [id]);

  if (isLoading) {
    return (
      <PublicLayout>
        <p className="dash-page__status">Đang tải bài viết…</p>
      </PublicLayout>
    );
  }

  if (notFound || !post) {
    return (
      <PublicLayout>
        <div className="blog-post__not-found">
          <p className="dash-page__eyebrow">404</p>
          <h1 className="blog-hero__title">Không tìm thấy bài viết</h1>
          <p className="blog-hero__tagline">Bài viết này không tồn tại hoặc chưa được xuất bản.</p>
          <Link to="/blog" className="public-shell__cta">Về trang bìa</Link>
        </div>
      </PublicLayout>
    );
  }

  return (
    <PublicLayout>
      <article className="blog-post">
        <p className="dash-page__eyebrow">{post.author?.fullName ?? 'Ẩn danh'}</p>
        <h1 className="blog-post__title">{post.title}</h1>
        {post.publishedDate && (
          <p className="blog-post__date">
            Xuất bản {new Date(post.publishedDate).toLocaleDateString('vi-VN')}
          </p>
        )}

        <div className="blog-post__content">
          {post.content?.split('\n').map((paragraph, i) => (
            <p key={i}>{paragraph}</p>
          ))}
        </div>

        <section className="blog-post__comments">
          <h2>Bình luận ({post.comments?.length ?? 0})</h2>
          {!post.comments || post.comments.length === 0 ? (
            <p className="dash-page__status">Chưa có bình luận nào.</p>
          ) : (
            <ul className="blog-comment-list">
              {post.comments.map((comment) => (
                <li key={comment.id} className="blog-comment">
                  <p className="blog-comment__author">{comment.userFullName}</p>
                  <p className="blog-comment__content">{comment.content}</p>
                </li>
              ))}
            </ul>
          )}
        </section>
      </article>
    </PublicLayout>
  );
}