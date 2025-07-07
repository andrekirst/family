import { test, expect } from '@playwright/test';

test.describe('Developer Tools', () => {
  test.describe('Health Dashboard', () => {
    test.beforeEach(async ({ page }) => {
      // Navigate to health dashboard
      await page.goto('/developer-tools/health-dashboard');
    });

    test('should display health dashboard page', async ({ page }) => {
      await expect(page.locator('h1')).toContainText('Health Check Dashboard');
      await expect(page.locator('p')).toContainText('System-Status und Service-Überwachung');
    });

    test('should have refresh button', async ({ page }) => {
      const refreshButton = page.locator('button:has-text("Aktualisieren")');
      await expect(refreshButton).toBeVisible();
      await expect(refreshButton).toHaveAttribute('aria-label', 'Gesundheitsstatus aktualisieren');
    });

    test('should have auto-refresh toggle', async ({ page }) => {
      const toggleButton = page.locator('button[aria-pressed]');
      await expect(toggleButton).toBeVisible();
      
      // Check initial state
      const initialState = await toggleButton.getAttribute('aria-pressed');
      
      // Click toggle
      await toggleButton.click();
      
      // State should change
      const newState = await toggleButton.getAttribute('aria-pressed');
      expect(newState).not.toBe(initialState);
    });

    test('should display overall system status', async ({ page }) => {
      await expect(page.locator('.text-2xl.font-bold')).toBeVisible();
      await expect(page.locator(':text("Services sind verfügbar")')).toBeVisible();
      await expect(page.locator(':text("Letzte Aktualisierung:")')).toBeVisible();
    });

    test('should handle refresh button click', async ({ page }) => {
      // Mock health check API
      await page.route('/api/health', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              name: 'Database',
              status: 'Healthy',
              responseTime: 15,
              lastChecked: new Date().toISOString()
            }
          ])
        });
      });

      const refreshButton = page.locator('button:has-text("Aktualisieren")');
      await refreshButton.click();

      // Should show loading state briefly
      await expect(page.locator('.animate-spin')).toBeVisible();
    });

    test('should display service cards when services are available', async ({ page }) => {
      // Mock health services
      await page.route('/api/health', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              name: 'Database',
              status: 'Healthy',
              responseTime: 15,
              lastChecked: new Date().toISOString(),
              details: { connectionString: 'postgres://...' }
            },
            {
              name: 'Redis Cache',
              status: 'Degraded',
              responseTime: 500,
              lastChecked: new Date().toISOString(),
              error: 'High response time'
            }
          ])
        });
      });

      await page.reload();

      // Should display service cards
      await expect(page.locator(':text("Database")')).toBeVisible();
      await expect(page.locator(':text("Redis Cache")')).toBeVisible();
      await expect(page.locator(':text("Gesund")')).toBeVisible();
      await expect(page.locator(':text("Beeinträchtigt")')).toBeVisible();
    });

    test('should handle error state', async ({ page }) => {
      // Mock error response
      await page.route('/api/health', route => route.abort('failed'));

      await page.reload();

      // Should show error message
      await expect(page.locator('.bg-red-50')).toBeVisible();
      await expect(page.locator(':text("Fehler beim Abrufen")')).toBeVisible();
    });

    test('should be accessible', async ({ page }) => {
      // Check heading hierarchy
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('h2')).toBeVisible();

      // Check ARIA labels
      const refreshButton = page.locator('button[aria-label]');
      await expect(refreshButton).toHaveAttribute('aria-label');

      // Check focus states
      await page.keyboard.press('Tab');
      await expect(page.locator('button:focus')).toBeVisible();
    });
  });

  test.describe('GraphQL Playground', () => {
    test.beforeEach(async ({ page }) => {
      // Navigate to GraphQL playground
      await page.goto('/developer-tools/graphql-playground');
    });

    test('should display GraphQL playground page', async ({ page }) => {
      await expect(page.locator('h1')).toContainText('GraphQL Playground');
      await expect(page.locator('p')).toContainText('Interaktive GraphQL-IDE');
    });

    test('should have open in new tab button', async ({ page }) => {
      const openButton = page.locator('button:has-text("In neuem Tab öffnen")');
      await expect(openButton).toBeVisible();
      await expect(openButton).toHaveAttribute('aria-label', 'GraphQL Playground in neuem Tab öffnen');
    });

    test('should display feature list', async ({ page }) => {
      await expect(page.locator(':text("GraphQL Playground Features")')).toBeVisible();
      await expect(page.locator(':text("Interaktive Query-Erstellung")')).toBeVisible();
      await expect(page.locator(':text("Schema-Explorer")')).toBeVisible();
      await expect(page.locator(':text("Query-History")')).toBeVisible();
      await expect(page.locator(':text("Variablen und HTTP-Header")')).toBeVisible();
    });

    test('should show iframe when GraphQL endpoint is available', async ({ page }) => {
      // Mock successful iframe load
      await page.route('/graphql/playground', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'text/html',
          body: '<html><body>GraphQL Playground</body></html>'
        });
      });

      await page.reload();

      // Should show iframe
      await expect(page.locator('iframe[title="GraphQL Playground"]')).toBeVisible();
    });

    test('should show loading state initially', async ({ page }) => {
      // Should show loading spinner
      await expect(page.locator('.animate-spin')).toBeVisible();
      await expect(page.locator(':text("wird geladen")')).toBeVisible();
    });

    test('should handle open in new tab click', async ({ page }) => {
      const openButton = page.locator('button:has-text("In neuem Tab öffnen")');
      
      // Listen for new page
      const [newPage] = await Promise.all([
        page.context().waitForEvent('page'),
        openButton.click()
      ]);

      // New page should open with GraphQL playground URL
      expect(newPage.url()).toContain('/graphql/playground');
    });

    test('should handle error state', async ({ page }) => {
      // Mock error in loading
      await page.route('/graphql/playground', route => route.abort('failed'));

      await page.reload();

      // Should show error message
      await expect(page.locator('.bg-red-50')).toBeVisible();
      await expect(page.locator(':text("Fehler beim Laden")')).toBeVisible();
    });

    test('should be responsive', async ({ page }) => {
      // Check responsive classes
      await expect(page.locator('.max-w-7xl')).toBeVisible();
      await expect(page.locator('.px-4.sm\\:px-6.lg\\:px-8')).toBeVisible();
    });

    test('should be accessible', async ({ page }) => {
      // Check heading hierarchy
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('h3')).toBeVisible();

      // Check iframe accessibility
      const iframe = page.locator('iframe');
      await expect(iframe).toHaveAttribute('title', 'GraphQL Playground');
      await expect(iframe).toHaveAttribute('aria-label');

      // Check button accessibility
      const openButton = page.locator('button[aria-label]');
      await expect(openButton).toHaveAttribute('aria-label');
    });
  });
});