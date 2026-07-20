import { useEffect, useId, useState } from "react";
import { NavLink } from "react-router-dom";

/**
 * Shared role portal shell with accessible mobile navigation.
 */
export default function RoleShell({ title, navLabel, links, children }) {
  const [open, setOpen] = useState(false);
  const navId = useId();

  useEffect(() => {
    function onKey(e) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, []);

  return (
    <div className="hs-shell">
      <header className="hs-shell__top">
        <h1 className="hs-shell__brand">{title}</h1>
        <button
          type="button"
          className="hs-btn hs-btn--secondary hs-menu-toggle"
          aria-expanded={open}
          aria-controls={navId}
          aria-label={open ? "Close navigation menu" : "Open navigation menu"}
          onClick={() => setOpen((v) => !v)}
        >
          Menu
        </button>
        <nav
          id={navId}
          className={`hs-shell__nav ${open ? "is-open" : ""}`}
          aria-label={navLabel}
          onClick={() => setOpen(false)}
        >
          {links.map((link) => (
            <NavLink key={link.to} to={link.to} end={link.end}>
              {link.label}
            </NavLink>
          ))}
        </nav>
      </header>
      <div className="hs-shell__body">{children}</div>
    </div>
  );
}
