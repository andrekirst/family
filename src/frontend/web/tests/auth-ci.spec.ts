import { test, expect } from '@playwright/test';

// Simplified tests optimized for CI/CD environments
test.describe('Authentication Tests - CI', () => {
  test.beforeEach(async ({ page }) => {
    await page.context().clearCookies();
  });

  test('should load login page', async ({ page }) => {
    await page.goto('/auth/login');
    await page.waitForSelector('mat-card-title', { timeout: 10000 });
    await expect(page.locator('mat-card-title')).toContainText('Family Login');
  });

  test('should redirect unauthenticated users', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/.*\/auth\/login.*/, { timeout: 10000 });
  });

  test('should show login form elements', async ({ page }) => {
    await page.goto('/auth/login');
    await page.waitForSelector('input[formControlName="email"]', { timeout: 10000 });
    
    await expect(page.locator('input[formControlName="email"]')).toBeVisible();
    await expect(page.locator('input[formControlName="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
    await expect(page.locator('button:has-text("Mit Keycloak anmelden")')).toBeVisible();
  });

  test('should validate email format', async ({ page }) => {
    await page.goto('/auth/login');
    await page.waitForSelector('input[formControlName="email"]', { timeout: 10000 });
    
    await page.locator('input[formControlName="email"]').fill('invalid-email');
    await page.locator('input[formControlName="email"]').blur();
    
    await expect(page.locator('mat-error')).toContainText('Bitte geben Sie eine gültige E-Mail ein');
  });

  test('should handle successful mock login', async ({ page }) => {
    await page.route('**/graphql', async route => {
      const request = route.request();
      const postData = request.postData();
      
      if (postData?.includes('directLogin')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              directLogin: {
                accessToken: 'mock-token',
                refreshToken: 'mock-refresh',
                user: { id: '1', email: 'test@example.com', fullName: 'Test User' }
              }
            }
          })
        });
      } else if (postData?.includes('currentUser')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              currentUser: { id: '1', email: 'test@example.com', fullName: 'Test User' }
            }
          })
        });
      } else {
        await route.continue();
      }
    });

    await page.goto('/auth/login');
    await page.waitForSelector('input[formControlName="email"]', { timeout: 10000 });
    
    await page.locator('input[formControlName="email"]').fill('test@example.com');
    await page.locator('input[formControlName="password"]').fill('password123');
    await page.locator('button[type="submit"]').click();
    
    await expect(page).toHaveURL(/.*\/dashboard.*/, { timeout: 15000 });
  });
});