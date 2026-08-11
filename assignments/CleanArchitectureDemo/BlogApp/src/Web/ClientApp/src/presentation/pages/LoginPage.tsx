// presentation/pages/LoginPage.tsx
import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../../composition';
import { AuthLayout } from '../layouts/AuthLayout';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const login = useAuthStore((s) => s.login);
  const isLoading = useAuthStore((s) => s.isLoading);
  const error = useAuthStore((s) => s.error);
  const navigate = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    try {
      await login(email, password);
      navigate('/');
    } catch {
      // error đã set trong store
    }
  }

  return (
    <AuthLayout
      eyebrow="Chào bạn trở lại"
      title="Đăng nhập"
      tagline="Vào bàn viết của bạn — bản thảo đang chờ."
      footer={
        <p className="auth-form__switch">
          Chưa có tài khoản? <Link to="/register">Tạo tài khoản</Link>
        </p>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        {error && <p className="auth-form__alert" role="alert">{error}</p>}

        <label className="auth-form__field">
          <span>Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ban@vidu.com"
            autoComplete="email"
            required
          />
        </label>

        <label className="auth-form__field">
          <span>Mật khẩu</span>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            autoComplete="current-password"
            required
          />
        </label>

        <button className="auth-form__submit" type="submit" disabled={isLoading}>
          {isLoading ? 'Đang đăng nhập…' : 'Đăng nhập'}
        </button>
      </form>
    </AuthLayout>
  );
}