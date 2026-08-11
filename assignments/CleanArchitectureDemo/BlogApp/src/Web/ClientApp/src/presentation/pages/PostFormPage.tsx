// presentation/pages/PostFormPage.tsx
import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { postApi } from '../../infrastructure/posts/postApi';
import { DashboardLayout } from '../layouts/DashboardLayout';

export function PostFormPage() {
  const { id } = useParams(); // Có id -> chế độ edit, không có -> chế độ tạo mới
  const isEditMode = Boolean(id);
  const navigate = useNavigate();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [isLoading, setIsLoading] = useState(isEditMode);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isEditMode) return;
    let cancelled = false;

    async function load() {
      try {
        const post = await postApi.getById(Number(id));
        if (!cancelled) {
          setTitle(post.title);
          setContent(post.content);
        }
      } catch {
        if (!cancelled) setError('Không tìm thấy bài viết.');
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [id, isEditMode]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);
    try {
      if (isEditMode) {
        await postApi.update(Number(id), { title, content });
      } else {
        await postApi.create({ title, content });
      }
      navigate('/');
    } catch {
      setError('Không thể lưu bài viết, vui lòng thử lại.');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <DashboardLayout>
      <div className="post-form-page">
        <p className="dash-page__eyebrow">{isEditMode ? 'Chỉnh sửa' : 'Bài viết mới'}</p>
        <h1 className="dash-page__title">{isEditMode ? 'Sửa bài viết' : 'Viết bài mới'}</h1>

        {isLoading ? (
          <p className="dash-page__status">Đang tải bài viết…</p>
        ) : (
          <form className="post-form" onSubmit={handleSubmit}>
            {error && <p className="auth-form__alert" role="alert">{error}</p>}

            <label className="auth-form__field">
              <span>Tiêu đề</span>
              <input value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={200} />
            </label>

            <label className="auth-form__field">
              <span>Nội dung</span>
              <textarea
                className="post-form__content"
                value={content}
                onChange={(e) => setContent(e.target.value)}
                rows={12}
                required
              />
            </label>

            <div className="post-form__actions">
              <button type="button" className="post-form__cancel" onClick={() => navigate('/')}>
                Huỷ
              </button>
              <button type="submit" className="auth-form__submit" disabled={isSaving}>
                {isSaving ? 'Đang lưu…' : 'Lưu bài viết'}
              </button>
            </div>
          </form>
        )}
      </div>
    </DashboardLayout>
  );
}