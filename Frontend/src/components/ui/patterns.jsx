import { useId, useState } from "react";

export function Tabs({ labels, panels }) {
  const [index, setIndex] = useState(0);
  const baseId = useId();

  function onKeyDown(event) {
    if (event.key === "ArrowRight") {
      event.preventDefault();
      setIndex((i) => (i + 1) % labels.length);
    } else if (event.key === "ArrowLeft") {
      event.preventDefault();
      setIndex((i) => (i - 1 + labels.length) % labels.length);
    }
  }

  return (
    <div>
      <div role="tablist" aria-label="Sections" onKeyDown={onKeyDown}>
        {labels.map((label, i) => (
          <button
            key={label}
            type="button"
            role="tab"
            id={`${baseId}-tab-${i}`}
            aria-selected={i === index}
            aria-controls={`${baseId}-panel-${i}`}
            tabIndex={i === index ? 0 : -1}
            className="hs-btn hs-btn--ghost"
            onClick={() => setIndex(i)}
          >
            {label}
          </button>
        ))}
      </div>
      {panels.map((panel, i) => (
        <div
          key={labels[i]}
          role="tabpanel"
          id={`${baseId}-panel-${i}`}
          aria-labelledby={`${baseId}-tab-${i}`}
          hidden={i !== index}
        >
          {panel}
        </div>
      ))}
    </div>
  );
}

export function Accordion({ items }) {
  const [open, setOpen] = useState(null);
  return (
    <div>
      {items.map((item, i) => {
        const expanded = open === i;
        return (
          <div key={item.title}>
            <h3>
              <button
                type="button"
                className="hs-btn hs-btn--ghost"
                aria-expanded={expanded}
                onClick={() => setOpen(expanded ? null : i)}
              >
                {item.title}
              </button>
            </h3>
            {expanded ? <div>{item.content}</div> : null}
          </div>
        );
      })}
    </div>
  );
}

export function Pagination({ page, pageCount, onChange }) {
  return (
    <nav aria-label="Pagination" className="hs-inline">
      <button
        type="button"
        className="hs-btn hs-btn--secondary"
        disabled={page <= 1}
        onClick={() => onChange(page - 1)}
      >
        Previous
      </button>
      <span aria-live="polite">
        Page {page} of {pageCount}
      </span>
      <button
        type="button"
        className="hs-btn hs-btn--secondary"
        disabled={page >= pageCount}
        onClick={() => onChange(page + 1)}
      >
        Next
      </button>
    </nav>
  );
}

export function FileUpload({ id = "file-upload", label = "Upload file", onChange, accept, hint }) {
  return (
    <div className="hs-field">
      <label className="hs-field__label" htmlFor={id}>
        {label}
      </label>
      {hint ? <p className="hs-field__hint" id={`${id}-hint`}>{hint}</p> : null}
      <input
        id={id}
        type="file"
        className="hs-input"
        accept={accept}
        aria-describedby={hint ? `${id}-hint` : undefined}
        onChange={onChange}
      />
    </div>
  );
}
