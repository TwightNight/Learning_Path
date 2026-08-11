// App.tsx
import { AppRouter } from './presentation/routes/AppRouter';
import { ToastContainer } from './presentation/notifications/ToastContainer';
import { ErrorBoundary } from './presentation/components/ErrorBoundary';

export default function App() {
  return (
    <ErrorBoundary>
      <AppRouter />
      <ToastContainer />
    </ErrorBoundary>
  );
}