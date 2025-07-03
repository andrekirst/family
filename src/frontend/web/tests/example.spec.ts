import { test, expect } from '@playwright/test';

test.describe('Basic Application Tests', () => {
  test('should load the application', async ({ page }) => {
    // Simple test to verify the app loads
    await page.goto('/');
    
    // Check that the page loads successfully (stays on root or redirects)
    await expect(page).toHaveURL(/.*localhost:4200.*/);
  });

  test('should have a page title', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page has any title (currently "Web")
    await expect(page).toHaveTitle(/\w+/);
  });
});