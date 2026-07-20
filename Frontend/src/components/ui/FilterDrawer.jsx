import { useId, useState } from "react";

/**
 * Mobile filter panel: one children tree; desktop shows filters inline via CSS.
 */
export default function FilterDrawer({ title = "Filters", children, defaultOpen = false }) {
  const [open, setOpen] = useState(defaultOpen);
  const panelId = useId();

  return (
    <div className={`portal-filter-drawer-shell ${open ? "is-open" : ""}`}>
      <button
        type="button"
        className="hs-btn hs-btn--secondary portal-filter-toggle"
        aria-expanded={open}
        aria-controls={panelId}
        onClick={() => setOpen((v) => !v)}
      >
        {open ? `Hide ${title}` : `Show ${title}`}
      </button>
      <div id={panelId} className="portal-filter-panel">
        {children}
      </div>
    </div>
  );
}
