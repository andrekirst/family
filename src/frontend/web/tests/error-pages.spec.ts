import { test, expect } from '@playwright/test';

test.describe('Error Pages', () => {
  test.describe('404 Not Found Page', () => {
    test.beforeEach(async ({ page }) => {
      // Navigate to non-existent page to trigger 404
      await page.goto('/non-existent-page');
    });

    test('should display 404 error page', async ({ page }) => {
      await expect(page.locator('h1')).toContainText('404');
      await expect(page.locator('h2')).toContainText('Seite nicht gefunden');
      await expect(page.locator('p')).toContainText('Die gesuchte Seite existiert leider nicht');
    });

    test('should have 404 illustration', async ({ page }) => {
      await expect(page.locator('svg.h-48.w-48')).toBeVisible();
    });

    test('should have navigation buttons', async ({ page }) => {
      await expect(page.locator('a[routerLink="/dashboard"]')).toBeVisible();
      await expect(page.locator('button[onclick="history.back()"]')).toBeVisible();
    });

    test('should navigate to dashboard when home button is clicked', async ({ page }) => {
      await page.click('a:has-text("Zur Startseite")');
      await expect(page).toHaveURL(/\/dashboard/);
    });

    test('should have support contact link', async ({ page }) => {
      const supportLink = page.locator('a[href="mailto:support@family.app"]');
      await expect(supportLink).toBeVisible();
      await expect(supportLink).toHaveAttribute('href', 'mailto:support@family.app');
    });

    test('should be responsive', async ({ page }) => {
      // Check responsive classes
      await expect(page.locator('.max-w-md')).toBeVisible();
      await expect(page.locator('.flex.flex-col.sm\\:flex-row')).toBeVisible();
      await expect(page.locator('.px-4.sm\\:px-6.lg\\:px-8')).toBeVisible();
    });

    test('should have proper accessibility', async ({ page }) => {
      // Check heading hierarchy
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('h2')).toBeVisible();

      // Check link accessibility
      const homeLink = page.locator('a:has-text("Zur Startseite")');
      await expect(homeLink).toHaveAccessibleName();

      const backButton = page.locator('button:has-text("Zurück")');
      await expect(backButton).toHaveAccessibleName();

      // Check focus states
      await page.keyboard.press('Tab');
      await expect(page.locator('a:focus, button:focus')).toBeVisible();
    });

    test('should have proper contrast and styling', async ({ page }) => {
      // Check for gradient background
      await expect(page.locator('.bg-gradient-to-br')).toBeVisible();
      
      // Check for proper button styling
      const homeButton = page.locator('a:has-text("Zur Startseite")');
      await expect(homeButton).toHaveClass(/bg-indigo-600/);
      
      const backButton = page.locator('button:has-text("Zurück")');
      await expect(backButton).toHaveClass(/bg-white/);
    });

    test('should support dark mode', async ({ page }) => {
      // Check for dark mode classes
      await expect(page.locator('.dark\\:from-gray-900')).toBeVisible();
      await expect(page.locator('.dark\\:text-gray-100')).toBeVisible();
    });

    test('should have animated elements', async ({ page }) => {
      // Check for animation classes
      await expect(page.locator('.animate-pulse')).toBeVisible();
      await expect(page.locator('.transition-colors')).toHaveCount(2); // Both buttons
    });
  });

  test.describe('500 Server Error Page', () => {
    test.beforeEach(async ({ page }) => {
      // Navigate to server error page
      await page.goto('/error/500');
    });

    test('should display 500 error page', async ({ page }) => {
      await expect(page.locator('h1')).toContainText('500');
      await expect(page.locator('h2')).toContainText('Serverfehler');
      await expect(page.locator('p')).toContainText('Es ist ein unerwarteter Fehler aufgetreten');
    });

    test('should have 500 illustration', async ({ page }) => {
      await expect(page.locator('svg.h-48.w-48')).toBeVisible();
    });

    test('should display system status', async ({ page }) => {
      await expect(page.locator(':text("System-Status: Fehler")')).toBeVisible();
      await expect(page.locator('.bg-red-500.animate-pulse')).toBeVisible();
    });

    test('should have action buttons', async ({ page }) => {
      await expect(page.locator('button:has-text("Seite neu laden")')).toBeVisible();
      await expect(page.locator('a:has-text("Zur Startseite")')).toBeVisible();
    });

    test('should reload page when reload button is clicked', async ({ page }) => {
      const reloadButton = page.locator('button:has-text("Seite neu laden")');
      
      // Mock page reload
      let reloadCalled = false;
      await page.evaluate(() => {
        const originalReload = window.location.reload;
        window.location.reload = function() {
          (window as any).reloadCalled = true;
        };
      });

      await reloadButton.click();
      
      // Check if reload was called (in a real test, this would reload the page)
      const result = await page.evaluate(() => (window as any).reloadCalled);
      expect(result).toBeTruthy();
    });

    test('should navigate to dashboard when home button is clicked', async ({ page }) => {
      await page.click('a:has-text("Zur Startseite")');
      await expect(page).toHaveURL(/\/dashboard/);
    });

    test('should have collapsible technical details', async ({ page }) => {
      const details = page.locator('details');
      const summary = page.locator('summary');
      
      await expect(details).toBeVisible();
      await expect(summary).toContainText('Technische Details anzeigen');
      
      // Click to expand
      await summary.click();
      
      // Should show technical information
      await expect(page.locator(':text("Zeitstempel:")')).toBeVisible();
      await expect(page.locator(':text("Fehler-ID:")')).toBeVisible();
      await expect(page.locator(':text("Status: Internal Server Error (500)")')).toBeVisible();
    });

    test('should generate unique error ID', async ({ page }) => {
      const errorIdText = await page.locator('details summary').click().then(() => 
        page.locator(':text("Fehler-ID:")').textContent()
      );
      
      expect(errorIdText).toMatch(/Fehler-ID: [A-Z0-9]{7}/);
    });

    test('should display timestamp in technical details', async ({ page }) => {
      await page.locator('summary').click();
      
      const timestampText = await page.locator(':text("Zeitstempel:")').textContent();
      expect(timestampText).toMatch(/Zeitstempel: \d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/);
    });

    test('should have support contact link', async ({ page }) => {
      const supportLink = page.locator('a[href="mailto:support@family.app"]');
      await expect(supportLink).toBeVisible();
      await expect(supportLink).toContainText('Kontaktieren Sie den Support');
    });

    test('should be responsive', async ({ page }) => {
      // Check responsive classes
      await expect(page.locator('.max-w-md')).toBeVisible();
      await expect(page.locator('.flex.flex-col.sm\\:flex-row')).toBeVisible();
    });

    test('should have proper accessibility', async ({ page }) => {
      // Check heading hierarchy
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('h2')).toBeVisible();

      // Check interactive elements accessibility
      const reloadButton = page.locator('button:has-text("Seite neu laden")');
      await expect(reloadButton).toHaveAccessibleName();

      const homeLink = page.locator('a:has-text("Zur Startseite")');
      await expect(homeLink).toHaveAccessibleName();

      // Check details/summary accessibility
      const summary = page.locator('summary');
      await expect(summary).toHaveClass(/cursor-pointer/);
    });

    test('should use red color scheme for error state', async ({ page }) => {
      // Check for red gradient background
      await expect(page.locator('.from-red-50')).toBeVisible();
      
      // Check for red styling on elements
      await expect(page.locator('.from-red-600')).toBeVisible(); // Heading gradient
      await expect(page.locator('.bg-red-600')).toBeVisible(); // Reload button
      await expect(page.locator('.text-red-600')).toBeVisible(); // Support link
    });

    test('should support dark mode', async ({ page }) => {
      // Check for dark mode classes
      await expect(page.locator('.dark\\:from-gray-900')).toBeVisible();
      await expect(page.locator('.dark\\:bg-red-500')).toBeVisible();
      await expect(page.locator('.dark\\:text-gray-100')).toBeVisible();
    });

    test('should have animated elements', async ({ page }) => {
      // Check for animation classes
      await expect(page.locator('h1.animate-pulse')).toBeVisible();
      await expect(page.locator('.bg-red-500.animate-pulse')).toBeVisible();
      await expect(page.locator('.transition-colors')).toHaveCount(2);
    });
  });

  test.describe('General Error Page Features', () => {
    test('should handle keyboard navigation', async ({ page }) => {
      await page.goto('/non-existent-page');
      
      // Test tab navigation
      await page.keyboard.press('Tab');
      await expect(page.locator('a:focus, button:focus')).toBeVisible();
      
      await page.keyboard.press('Tab');
      await expect(page.locator('a:focus, button:focus')).toBeVisible();
    });

    test('should have proper meta tags for SEO', async ({ page }) => {
      await page.goto('/non-existent-page');
      
      // Check page title
      await expect(page).toHaveTitle(/404|Not Found/);
    });

    test('should work without JavaScript', async ({ page, context }) => {
      // Disable JavaScript
      await context.setJavaScriptEnabled(false);
      await page.goto('/non-existent-page');
      
      // Basic content should still be visible
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('h2')).toBeVisible();
    });
  });
});