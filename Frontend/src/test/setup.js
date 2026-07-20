import React from 'react';
import '@testing-library/jest-dom/vitest';
import { afterEach, vi } from 'vitest';
import { cleanup } from '@testing-library/react';

// Ensure classic JSX runtime works for components that rely on the automatic transform.
globalThis.React = React;

afterEach(() => {
    cleanup();
    localStorage.clear();
    vi.restoreAllMocks();
});
