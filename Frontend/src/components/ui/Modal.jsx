import { useEffect, useId, useRef } from "react";
import { Button } from "./primitives";

export default function Modal({
  open,
  title,
  children,
  onClose,
  confirmLabel,
  onConfirm,
  dangerous = false
}) {
  const titleId = useId();
  const dialogRef = useRef(null);
  const previouslyFocused = useRef(null);

  useEffect(() => {
    if (!open) return undefined;
    previouslyFocused.current = document.activeElement;
    const node = dialogRef.current;
    const focusable = node?.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    focusable?.[0]?.focus();

    function onKeyDown(event) {
      if (event.key === "Escape") {
        event.preventDefault();
        onClose?.();
        return;
      }
      if (event.key !== "Tab" || !focusable?.length) return;
      const first = focusable[0];
      const last = focusable[focusable.length - 1];
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    }

    document.addEventListener("keydown", onKeyDown);
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = previousOverflow;
      previouslyFocused.current?.focus?.();
    };
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div className="hs-modal-backdrop" role="presentation" onMouseDown={(e) => {
      if (e.target === e.currentTarget) onClose?.();
    }}>
      <div
        ref={dialogRef}
        className="hs-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
      >
        <h2 id={titleId} className="hs-modal__title">
          {title}
        </h2>
        <div>{children}</div>
        <div className="hs-inline" style={{ marginTop: "1rem", justifyContent: "flex-end" }}>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          {onConfirm ? (
            <Button variant={dangerous ? "danger" : "primary"} onClick={onConfirm}>
              {confirmLabel || "Confirm"}
            </Button>
          ) : null}
        </div>
      </div>
    </div>
  );
}
