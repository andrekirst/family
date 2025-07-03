import { test, expect } from '@playwright/test';

test.describe('Authentication Tests - Simplified', () => {
  test.beforeEach(async ({ page }) => {
    // Clear any existing authentication state
    await page.context().clearCookies();
  });

  test.describe('Login Page', () => {
    test('should display login form', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Verify login form elements are present
      await expect(page.locator('mat-card-title')).toContainText('Family Login');
      await expect(page.locator('input[type="email"]')).toBeVisible();
      await expect(page.locator('input[type="password"]')).toBeVisible();
      await expect(page.locator('button[type="submit"]')).toBeVisible();
      await expect(page.locator('button:has-text("Mit Keycloak anmelden")')).toBeVisible();
    });

    test('should show validation errors for empty form', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Try to submit empty form
      await page.locator('button[type="submit"]').click();
      
      // Check that submit button is disabled due to validation
      await expect(page.locator('button[type="submit"]')).toBeDisabled();
    });

    test('should show validation error for invalid email', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Enter invalid email
      await page.locator('input[type="email"]').fill('invalid-email');
      await page.locator('input[type="email"]').blur();
      
      // Check for email validation error
      await expect(page.locator('mat-error')).toContainText('Bitte geben Sie eine gültige E-Mail ein');
    });

    test('should toggle password visibility', async ({ page }) => {
      await page.goto('/auth/login');
      
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

  test.describe('Navigation', () => {
    test('should redirect to login when not authenticated', async ({ page }) => {
      await page.goto('/');
      
      // Should redirect to login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });

    test('should redirect to login when accessing protected route', async ({ page }) => {
      await page.goto('/dashboard');
      
      // Should redirect to login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });
  });

  test.describe('Mock Authentication', () => {
    test('should handle successful login with mocked response', async ({ page }) => {
      // Mock the GraphQL response for successful login
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
                  success: true,
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
      
      // Fill in login form
      await page.locator('input[type="email"]').fill('test@example.com');
      await page.locator('input[type="password"]').fill('password123');
      
      // Submit form
      await page.locator('button[type="submit"]').click();
      
      // Should redirect to dashboard
      await expect(page).toHaveURL(/.*\/dashboard.*/);
    });

    test('should handle login failure with mocked response', async ({ page }) => {
      // Mock the GraphQL response for login failure
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
                  success: false,
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
      
      // Fill in login form
      await page.locator('input[type="email"]').fill('test@example.com');
      await page.locator('input[type="password"]').fill('wrongpassword');
      
      // Submit form
      await page.locator('button[type="submit"]').click();
      
      // Should show error message
      await expect(page.locator('simple-snack-bar')).toContainText('Invalid credentials');
    });
  });

  test.describe('OAuth Flow', () => {
    test('should initiate OAuth login', async ({ page }) => {
      // Mock the GraphQL response for OAuth initiation
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('initiateLogin')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                initiateLogin: {
                  loginUrl: 'http://localhost:8080/auth/realms/family/protocol/openid-connect/auth?state=mock-state',
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
      
      // Track navigation
      const navigationPromise = page.waitForNavigation();
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Should redirect to Keycloak
      await navigationPromise;
      await expect(page).toHaveURL(/.*localhost:8080.*/);
    });

    test('should handle OAuth callback', async ({ page }) => {
      // Mock the GraphQL response for OAuth completion
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('completeLogin')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                completeLogin: {
                  success: true,
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

      // First, set up session storage to simulate OAuth state
      await page.goto('/auth/login');
      await page.evaluate(() => {
        try {
          sessionStorage.setItem('oauth_state', 'mock-state-123');
        } catch (e) {
          // Ignore storage errors
        }
      });
      
      // Navigate to callback URL
      await page.goto('/auth/callback?code=mock-auth-code&state=mock-state-123');
      
      // Should redirect to dashboard
      await expect(page).toHaveURL(/.*\/dashboard.*/);
    });
  });
});