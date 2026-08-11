// presentation/pages/RegisterPage.tsx
import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../../composition';
import { AuthLayout } from '../layouts/AuthLayout';

export function RegisterPage() {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    userName: '',
    email: '',
    password: '',
  });
  const register = useAuthStore((s) => s.register);
  const isLoading = useAuthStore((s) => s.isLoading);
  const error = useAuthStore((s) => s.error);
  const navigate = useNavigate();

  function updateField(field: keyof typeof form) {
    return (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    try {
      await register(form);
      navigate('/login');
    } catch {
      // error đã set trong store
    }
  }

  return (
    <AuthLayout
      eyebrow="Bắt đầu viết"
      title="Tạo tài khoản"
      tagline="Mỗi tài khoản là một bàn viết riêng — của bạn, cho bạn."
      footer={
        <p className="auth-form__switch">
          Đã có tài khoản? <Link to="/login">Đăng nhập</Link>
        </p>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        {error && <p className="auth-form__alert" role="alert">{error}</p>}

        <div className="auth-form__row">
          <label className="auth-form__field">
            <span>Họ</span>
            <input value={form.lastName} onChange={updateField('lastName')} required />
          </label>
          <label className="auth-form__field">
            <span>Tên</span>
            <input value={form.firstName} onChange={updateField('firstName')} required />
          </label>
        </div>

        <label className="auth-form__field">
          <span>Tên đăng nhập</span>
          <input value={form.userName} onChange={updateField('userName')} required />
        </label>

        <label className="auth-form__field">
          <span>Email</span>
          <input
            type="email"
            value={form.email}
            onChange={updateField('email')}
            placeholder="ban@vidu.com"
            required
          />
        </label>

        <label className="auth-form__field">
          <span>Mật khẩu</span>
          <input
            type="password"
            value={form.password}
            onChange={updateField('password')}
            placeholder="Tối thiểu 6 ký tự"
            minLength={6}
            required
          />
        </label>

        <button className="auth-form__submit" type="submit" disabled={isLoading}>
          {isLoading ? 'Đang tạo tài khoản…' : 'Tạo tài khoản'}
        </button>
      </form>
    </AuthLayout>
  );
}