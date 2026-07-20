import { forwardRef } from "react";

export function Button({
  children,
  variant = "primary",
  type = "button",
  className = "",
  loading = false,
  disabled = false,
  ...props
}) {
  return (
    <button
      type={type}
      className={`hs-btn hs-btn--${variant} ${className}`.trim()}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...props}
    >
      {loading && <span className="hs-spinner" aria-hidden="true" />}
      {loading ? <span className="hs-sr-only">Loading</span> : null}
      {children}
    </button>
  );
}

export const Input = forwardRef(function Input(
  { id, label, hint, error, className = "", ...props },
  ref
) {
  const describedBy = [
    hint ? `${id}-hint` : null,
    error ? `${id}-error` : null
  ]
    .filter(Boolean)
    .join(" ") || undefined;

  return (
    <div className={`hs-field ${className}`.trim()}>
      {label ? (
        <label className="hs-field__label" htmlFor={id}>
          {label}
          {props.required ? <span aria-hidden="true"> *</span> : null}
        </label>
      ) : null}
      <input
        ref={ref}
        id={id}
        className="hs-input"
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        {...props}
      />
      {hint ? (
        <p id={`${id}-hint`} className="hs-field__hint">
          {hint}
        </p>
      ) : null}
      {error ? (
        <p id={`${id}-error`} className="hs-field__error" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  );
});

export function Alert({ variant = "info", title, children, role }) {
  const alertRole = role || (variant === "error" ? "alert" : "status");
  return (
    <div className={`hs-alert hs-alert--${variant}`} role={alertRole}>
      {title ? <strong>{title}. </strong> : null}
      {children}
    </div>
  );
}

export function StatusBadge({ tone = "neutral", label, children }) {
  const text = label || children;
  return (
    <span className={`hs-badge hs-badge--${tone}`}>
      <span aria-hidden="true">●</span>
      <span>{text}</span>
    </span>
  );
}

export function EmptyState({ title, children, action }) {
  return (
    <div className="hs-empty" role="status">
      <h2>{title}</h2>
      <div>{children}</div>
      {action || null}
    </div>
  );
}

export function ErrorState({ title = "Something went wrong", children, onRetry }) {
  return (
    <div className="hs-error-state" role="alert">
      <h2>{title}</h2>
      <div>{children}</div>
      {onRetry ? (
        <Button variant="secondary" onClick={onRetry} style={{ marginTop: "1rem" }}>
          Try again
        </Button>
      ) : null}
    </div>
  );
}

export function Spinner({ label = "Loading" }) {
  return (
    <span className="hs-inline" role="status" aria-live="polite">
      <span className="hs-spinner" aria-hidden="true" />
      <span>{label}</span>
    </span>
  );
}

export function PageHeader({ title, description, actions }) {
  return (
    <header className="hs-page-header">
      <div className="hs-inline" style={{ justifyContent: "space-between" }}>
        <div>
          <h1>{title}</h1>
          {description ? <p>{description}</p> : null}
        </div>
        {actions || null}
      </div>
    </header>
  );
}

export function ContentContainer({ children, className = "" }) {
  return <div className={`hs-page ${className}`.trim()}>{children}</div>;
}

export function SkipLink({ href = "#main-content" }) {
  return (
    <a className="hs-skip-link" href={href}>
      Skip to main content
    </a>
  );
}
