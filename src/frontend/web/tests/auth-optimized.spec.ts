import { test, expect } from '@playwright/test';

test.describe('Authentication Tests - Optimized', () => {
  test.beforeEach(async ({ page }) => {
    // Ensure clean state
    await page.context().clearCookies();
    
    // Set up GraphQL mocking first to prevent network delays
    await page.route('**/graphql', async route => {
      await route.continue();
    });
  });

  test.describe('Basic UI Tests', () => {
    test('should display login form elements', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Wait for Angular to load
      await page.waitForSelector('mat-card-title', { timeout: 15000 });
      
      // Verify login form elements are present
      await expect(page.locator('mat-card-title')).toContainText('Family Login');
      await expect(page.locator('input[formControlName="email"]')).toBeVisible();
      await expect(page.locator('input[formControlName="password"]')).toBeVisible();
      await expect(page.locator('button[type="submit"]')).toBeVisible();
      await expect(page.locator('button:has-text("Mit Keycloak anmelden")')).toBeVisible();
    });

    test('should have disabled submit button initially', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Wait for form to load
      await page.waitForSelector('button[type="submit"]', { timeout: 15000 });
      
      // Check that submit button is disabled for empty form
      await expect(page.locator('button[type="submit"]')).toBeDisabled();
    });

    test('should enable submit button with valid input', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Wait for form to load
      await page.waitForSelector('input[formControlName="email"]', { timeout: 15000 });
      
      // Fill in valid form data
      await page.locator('input[formControlName="email"]').fill('test@example.com');
      await page.locator('input[formControlName="password"]').fill('password123');
      
      // Submit button should be enabled
      await expect(page.locator('button[type="submit"]')).toBeEnabled();
    });

    test('should show email validation error', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Wait for form to load
      await page.waitForSelector('input[formControlName="email"]', { timeout: 15000 });
      
      // Enter invalid email and trigger validation
      await page.locator('input[formControlName="email"]').fill('invalid-email');
      await page.locator('input[formControlName="email"]').blur();
      
      // Wait for validation error to appear
      await page.waitForSelector('mat-error', { timeout: 5000 });
      await expect(page.locator('mat-error')).toContainText('Bitte geben Sie eine gültige E-Mail ein');
    });

    test('should toggle password visibility', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Wait for form to load
      await page.waitForSelector('input[formControlName="password"]', { timeout: 15000 });
      
      const passwordInput = page.locator('input[formControlName="password"]');
      const toggleButton = page.locator('button[matSuffix]');
      
      // Initially password should be hidden
      await expect(passwordInput).toHaveAttribute('type', 'password');
      
      // Click toggle button
      await toggleButton.click();
      
      // Password should now be visible
      await expect(passwordInput).toHaveAttribute('type', 'text');
    });
  });

  test.describe('Navigation Tests', () => {
    test('should redirect unauthenticated users to login', async ({ page }) => {
      await page.goto('/');
      
      // Should redirect to login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/, { timeout: 15000 });
    });

    test('should redirect protected routes to login', async ({ page }) => {
      await page.goto('/dashboard');
      
      // Should redirect to login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/, { timeout: 15000 });
    });
  });

  test.describe('Mock Authentication Tests', () => {
    test('should handle successful login flow', async ({ page }) => {
      // Set up GraphQL mocking before navigation
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
                  accessToken: 'mock-access-token',
                  refreshToken: 'mock-refresh-token',
                  user: {
                    id: '1',
                    email: 'test@example.com',
                    fullName: 'Test User'
                  }
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
                currentUser: {
                  id: '1',
                  email: 'test@example.com',
                  fullName: 'Test User'
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/auth/login');
      
      // Wait for form to be ready
      await page.waitForSelector('input[formControlName="email"]', { timeout: 15000 });
      
      // Fill in login form
      await page.locator('input[formControlName="email"]').fill('test@example.com');
      await page.locator('input[formControlName="password"]').fill('password123');
      
      // Submit form
      await page.locator('button[type="submit"]').click();
      
      // Should redirect to dashboard
      await expect(page).toHaveURL(/.*\/dashboard.*/, { timeout: 20000 });
    });

    test('should show error for failed login', async ({ page }) => {
      // Set up GraphQL mocking for failure
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
                  errors: ['Invalid credentials']
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/auth/login');
      
      // Wait for form to be ready
      await page.waitForSelector('input[formControlName="email"]', { timeout: 15000 });
      
      // Fill in login form
      await page.locator('input[formControlName="email"]').fill('test@example.com');
      await page.locator('input[formControlName="password"]').fill('wrongpassword');
      
      // Submit form
      await page.locator('button[type="submit"]').click();
      
      // Should show error snackbar
      await expect(page.locator('simple-snack-bar')).toContainText('Invalid credentials', { timeout: 10000 });
    });
  });

  test.describe('OAuth Flow Tests', () => {
    test('should handle OAuth initiation', async ({ page }) => {
      // Set up GraphQL mocking for OAuth
      let oauthInitiated = false;
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('initiateLogin')) {
          oauthInitiated = true;
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                initiateLogin: {
                  loginUrl: 'http://localhost:8080/auth/test',
                  state: 'mock-state'
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/auth/login');
      
      // Wait for OAuth button to be ready
      await page.waitForSelector('button:has-text("Mit Keycloak anmelden")', { timeout: 15000 });
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Wait a moment for the GraphQL call to complete
      await page.waitForTimeout(2000);
      
      // Verify that OAuth initiation was called
      expect(oauthInitiated).toBe(true);
    });
  });

  test.describe('Authenticated State Tests', () => {
    test('should show user menu when authenticated', async ({ page }) => {
      // Set up mock authentication
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('currentUser')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                currentUser: {
                  id: '1',
                  email: 'test@example.com',
                  fullName: 'Test User'
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });

      // Set up token in localStorage before navigation
      await page.goto('/auth/login');
      await page.evaluate(() => {
        try {
          localStorage.setItem('accessToken', 'mock-access-token');
        } catch (e) {
          // Ignore storage errors
        }
      });
      
      // Navigate to protected route
      await page.goto('/dashboard');
      
      // Wait for layout to load
      await page.waitForSelector('button[mat-icon-button]:has(mat-icon:text("account_circle"))', { timeout: 20000 });
      
      // Should show user menu button
      await expect(page.locator('button[mat-icon-button]:has(mat-icon:text("account_circle"))')).toBeVisible();
      
      // Click user menu
      await page.locator('button[mat-icon-button]:has(mat-icon:text("account_circle"))').click();
      
      // Should show user info and logout option
      await expect(page.locator('.user-email')).toContainText('test@example.com');
      await expect(page.locator('button[mat-menu-item]:has-text("Abmelden")')).toBeVisible();
    });
  });
});