import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  projects: [{ name: 'chromium', use: { browserName: 'chromium' } }],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  webServer: process.env.CI
    ? {
        command: 'npm run dev -- --host 0.0.0.0',
        url: 'http://localhost:5173',
        reuseExistingServer: false,
      }
    : undefined,
});
