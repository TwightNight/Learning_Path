// presentation/components/ErrorBoundary.tsx
import { Component, type ErrorInfo, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: (error: Error, reset: () => void) => ReactNode;
}

interface State {
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    // Điểm nối log tập trung sau này (Sentry, App Insights...)
    console.error('Unhandled render error:', error, info.componentStack);
  }

  reset = () => this.setState({ error: null });

  render() {
    const { error } = this.state;
    if (!error) return this.props.children;

    if (this.props.fallback) return this.props.fallback(error, this.reset);

    return (
      <div className="error-boundary-fallback">
        <h1>Đã có lỗi xảy ra</h1>
        <p>Ứng dụng gặp sự cố ngoài dự kiến. Vui lòng thử tải lại trang.</p>
        <button onClick={() => window.location.reload()}>Tải lại trang</button>
      </div>
    );
  }
}