import { test, expect } from '@playwright/test';

test.describe('Authentication Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Clear any existing authentication state
    await page.context().clearCookies();
    
    // Navigate to a page first to ensure storage is accessible
    await page.goto('/auth/login');
    
    // Clear storage after navigation
    await page.evaluate(() => {
      try {
        localStorage.clear();
        sessionStorage.clear();
      } catch (e) {
        // Ignore storage access errors
      }
    });
  });

  test.describe('Login Functionality', () => {
    test('should display login form when not authenticated', async ({ page }) => {
      await page.goto('/');
      
      // Should redirect to login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
      
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

    test('should show validation error for missing email', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Focus and blur email field without entering value
      await page.locator('input[type="email"]').focus();
      await page.locator('input[type="email"]').blur();
      
      // Check for required field error
      await expect(page.locator('mat-error')).toContainText('E-Mail ist erforderlich');
    });

    test('should show validation error for missing password', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Focus and blur password field without entering value
      await page.locator('input[type="password"]').focus();
      await page.locator('input[type="password"]').blur();
      
      // Check for required field error
      await expect(page.locator('mat-error')).toContainText('Passwort ist erforderlich');
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
      
      // Click again to hide
      await toggleButton.click();
      
      // Password should be hidden again
      await expect(passwordInput).toHaveAttribute('type', 'password');
    });

    test('should handle direct login attempt', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Fill in login form
      await page.locator('input[type="email"]').fill('test@example.com');
      await page.locator('input[type="password"]').fill('password123');
      
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
      
      // Submit form
      await page.locator('button[type="submit"]').click();
      
      // Wait for and verify error message
      await expect(page.locator('simple-snack-bar')).toContainText('Invalid credentials');
    });

    test('should handle successful direct login', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Fill in login form
      await page.locator('input[type="email"]').fill('test@example.com');
      await page.locator('input[type="password"]').fill('password123');
      
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
      
      // Submit form
      await page.locator('button[type="submit"]').click();
      
      // Should redirect to dashboard
      await expect(page).toHaveURL(/.*\/dashboard.*/);
      
      // Verify success message
      await expect(page.locator('simple-snack-bar')).toContainText('Erfolgreich angemeldet!');
    });

    test('should handle OAuth login initiation', async ({ page }) => {
      await page.goto('/auth/login');
      
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
                  loginUrl: 'http://localhost:8080/auth/realms/family/protocol/openid-connect/auth?client_id=family-web&redirect_uri=http://localhost:4200/auth/callback&response_type=code&state=mock-state-123',
                  state: 'mock-state-123'
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Track navigation
      const navigationPromise = page.waitForNavigation();
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Should redirect to Keycloak
      await navigationPromise;
      await expect(page).toHaveURL(/.*localhost:8080.*/);
    });

    test('should handle OAuth login failure', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Mock the GraphQL response for OAuth failure
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
                  errors: ['OAuth service unavailable']
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Should show error message
      await expect(page.locator('simple-snack-bar')).toContainText('OAuth service unavailable');
    });

    test('should redirect authenticated users away from login', async ({ page }) => {
      // Set up authenticated state
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'mock-access-token');
      });
      
      // Mock the GraphQL response for current user
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
      
      // Try to navigate to login
      await page.goto('/auth/login');
      
      // Should redirect to dashboard
      await expect(page).toHaveURL(/.*\/dashboard.*/);
    });
  });

  test.describe('Logout Functionality', () => {
    test.beforeEach(async ({ page }) => {
      // Set up authenticated state
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'mock-access-token');
        localStorage.setItem('refreshToken', 'mock-refresh-token');
      });
      
      // Mock the GraphQL response for current user
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
    });

    test('should display user menu when authenticated', async ({ page }) => {
      await page.goto('/dashboard');
      
      // Check that user menu button is visible
      await expect(page.locator('button[mat-icon-button]:has(mat-icon:text("account_circle"))')).toBeVisible();
      
      // Click user menu
      await page.locator('button[mat-icon-button]:has(mat-icon:text("account_circle"))').click();
      
      // Verify user menu content
      await expect(page.locator('.user-name')).toContainText('Test User');
      await expect(page.locator('.user-email')).toContainText('test@example.com');
      await expect(page.locator('button[mat-menu-item]:has-text("Abmelden")')).toBeVisible();
    });

    test('should handle successful logout', async ({ page }) => {
      await page.goto('/dashboard');
      
      // Mock the GraphQL response for logout
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('logout')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                logout: {
                  success: true
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
      
      // Click user menu
      await page.locator('button[mat-icon-button]:has(mat-icon:text("account_circle"))').click();
      
      // Click logout button
      await page.locator('button[mat-menu-item]:has-text("Abmelden")').click();
      
      // Should redirect to login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
      
      // Verify success message
      await expect(page.locator('simple-snack-bar')).toContainText('Erfolgreich abgemeldet');
      
      // Verify tokens are cleared
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refreshToken'));
      expect(accessToken).toBeNull();
      expect(refreshToken).toBeNull();
    });

    test('should handle logout error gracefully', async ({ page }) => {
      await page.goto('/dashboard');
      
      // Mock the GraphQL response for logout error
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('logout')) {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'Logout failed' }]
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
      
      // Click user menu
      await page.locator('button[mat-icon-button]:has(mat-icon:text("account_circle"))').click();
      
      // Click logout button
      await page.locator('button[mat-menu-item]:has-text("Abmelden")').click();
      
      // Should still redirect to login page (logout cleanup happens regardless)
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
      
      // Verify tokens are cleared even on error
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refreshToken'));
      expect(accessToken).toBeNull();
      expect(refreshToken).toBeNull();
    });

    test('should handle logout when no token exists', async ({ page }) => {
      // Clear tokens
      await page.evaluate(() => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
      });
      
      await page.goto('/dashboard');
      
      // Since no token exists, should redirect to login immediately
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });
  });

  test.describe('Authentication Guards', () => {
    test('should redirect unauthenticated users to login', async ({ page }) => {
      // Try to access protected route without authentication
      await page.goto('/dashboard');
      
      // Should redirect to login
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });

    test('should allow authenticated users to access protected routes', async ({ page }) => {
      // Set up authenticated state
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'mock-access-token');
      });
      
      // Mock the GraphQL response for current user
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
      
      // Navigate to protected route
      await page.goto('/dashboard');
      
      // Should stay on dashboard
      await expect(page).toHaveURL(/.*\/dashboard.*/);
    });

    test('should redirect to login when token is invalid', async ({ page }) => {
      // Set up invalid token
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'invalid-token');
      });
      
      // Mock the GraphQL response for invalid token
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('currentUser')) {
          await route.fulfill({
            status: 401,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'Unauthorized' }]
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Try to access protected route
      await page.goto('/dashboard');
      
      // Should redirect to login
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
      
      // Token should be cleared
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      expect(accessToken).toBeNull();
    });
  });
});