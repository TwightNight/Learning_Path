import type { JSX } from "react/jsx-runtime";

// presentation/components/RoleBadge.tsx
interface RoleStyle {
  color: string;
  label: string;
  glyph: JSX.Element;
}

const ROLE_STYLE: Record<string, RoleStyle> = {
  Admin: {
    color: 'var(--plum)',
    label: 'Admin',
    glyph: <path d="M12 3l6 2.4v5.2c0 4.2-2.6 7.4-6 8.8-3.4-1.4-6-4.6-6-8.8V5.4L12 3z" />,
  },
  Author: {
    color: 'var(--forest)',
    label: 'Author',
    glyph: <path d="M4 20l1.2-4.6L15.8 4.8a1.4 1.4 0 0 1 2 0l1.4 1.4a1.4 1.4 0 0 1 0 2L8.6 18.8 4 20z" />,
  },
};

interface RoleBadgeProps {
  role: string;
  size?: number;
}

export function RoleBadge({ role, size = 36 }: RoleBadgeProps) {
  const style = ROLE_STYLE[role] ?? { color: 'var(--muted)', label: role, glyph: <></> };

  return (
    <span className="role-badge" title={style.label}>
      <svg viewBox="0 0 24 24" width={size} height={size} fill="none">
        <circle cx="12" cy="12" r="11" stroke={style.color} strokeWidth="1.4" />
        <circle cx="12" cy="12" r="8.4" stroke={style.color} strokeWidth="0.8" strokeDasharray="1.6 1.6" />
        <g fill={style.color}>{style.glyph}</g>
      </svg>
      <span className="role-badge__label" style={{ color: style.color }}>
        {style.label}
      </span>
    </span>
  );
}