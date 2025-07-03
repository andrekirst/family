import { test, expect } from '@playwright/test';

test.describe('Keycloak OAuth Integration Tests', () => {
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

  test.describe('OAuth Callback Flow', () => {
    test('should handle successful OAuth callback', async ({ page }) => {
      // Set up OAuth state in session storage (simulates initiated OAuth flow)
      await page.evaluate(() => {
        sessionStorage.setItem('oauth_state', 'mock-state-123');
      });
      
      // Mock the GraphQL response for successful OAuth completion
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
      
      // Navigate to callback URL with authorization code and state
      await page.goto('/auth/callback?code=mock-auth-code&state=mock-state-123');
      
      // Should redirect to dashboard after successful authentication
      await expect(page).toHaveURL(/.*\/dashboard.*/);
      
      // Verify tokens are stored
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refreshToken'));
      expect(accessToken).toBe('mock-access-token');
      expect(refreshToken).toBe('mock-refresh-token');
      
      // Verify OAuth state is cleaned up
      const oauthState = await page.evaluate(() => sessionStorage.getItem('oauth_state'));
      expect(oauthState).toBeNull();
    });

    test('should handle OAuth callback with invalid state', async ({ page }) => {
      // Set up different OAuth state in session storage
      await page.evaluate(() => {
        sessionStorage.setItem('oauth_state', 'different-state');
      });
      
      // Navigate to callback URL with mismatched state
      await page.goto('/auth/callback?code=mock-auth-code&state=mock-state-123');
      
      // Should show error message for invalid state
      await expect(page.locator('simple-snack-bar')).toContainText('Invalid state parameter');
      
      // Should stay on callback page or redirect to login
      await expect(page).toHaveURL(/.*\/auth\/(callback|login).*/);
    });

    test('should handle OAuth callback with missing state', async ({ page }) => {
      // No OAuth state in session storage
      
      // Navigate to callback URL without session state
      await page.goto('/auth/callback?code=mock-auth-code&state=mock-state-123');
      
      // Should show error message for missing state
      await expect(page.locator('simple-snack-bar')).toContainText('Invalid state parameter');
    });

    test('should handle OAuth callback with missing authorization code', async ({ page }) => {
      // Set up OAuth state in session storage
      await page.evaluate(() => {
        sessionStorage.setItem('oauth_state', 'mock-state-123');
      });
      
      // Navigate to callback URL without authorization code
      await page.goto('/auth/callback?state=mock-state-123');
      
      // Should show error message for missing authorization code
      await expect(page.locator('simple-snack-bar')).toContainText('Authorization code not provided');
    });

    test('should handle OAuth callback with server error', async ({ page }) => {
      // Set up OAuth state in session storage
      await page.evaluate(() => {
        sessionStorage.setItem('oauth_state', 'mock-state-123');
      });
      
      // Mock the GraphQL response for server error
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('completeLogin')) {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'OAuth completion failed' }]
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Navigate to callback URL
      await page.goto('/auth/callback?code=mock-auth-code&state=mock-state-123');
      
      // Should show error message
      await expect(page.locator('simple-snack-bar')).toContainText('Network error occurred');
      
      // Should redirect to login
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });

    test('should handle OAuth callback with invalid authorization code', async ({ page }) => {
      // Set up OAuth state in session storage
      await page.evaluate(() => {
        sessionStorage.setItem('oauth_state', 'mock-state-123');
      });
      
      // Mock the GraphQL response for invalid authorization code
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
                  success: false,
                  errors: ['Invalid authorization code']
                }
              }
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Navigate to callback URL with invalid authorization code
      await page.goto('/auth/callback?code=invalid-auth-code&state=mock-state-123');
      
      // Should show error message
      await expect(page.locator('simple-snack-bar')).toContainText('Invalid authorization code');
      
      // Should redirect to login
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });

    test('should handle OAuth callback error parameter from Keycloak', async ({ page }) => {
      // Set up OAuth state in session storage
      await page.evaluate(() => {
        sessionStorage.setItem('oauth_state', 'mock-state-123');
      });
      
      // Navigate to callback URL with error parameter (Keycloak sends this on user cancellation)
      await page.goto('/auth/callback?error=access_denied&error_description=User%20cancelled%20login&state=mock-state-123');
      
      // Should show error message
      await expect(page.locator('simple-snack-bar')).toContainText('User cancelled login');
      
      // Should redirect to login
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });
  });

  test.describe('Token Refresh Flow', () => {
    test('should handle successful token refresh', async ({ page }) => {
      // Set up tokens in localStorage
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'expired-access-token');
        localStorage.setItem('refreshToken', 'valid-refresh-token');
      });
      
      // Mock the GraphQL response for token refresh
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('refreshToken')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                refreshToken: {
                  success: true,
                  accessToken: 'new-access-token',
                  refreshToken: 'new-refresh-token'
                }
              }
            })
          });
        } else if (postData?.includes('currentUser')) {
          // First call fails with expired token
          const authHeader = request.headers()['authorization'];
          if (authHeader === 'Bearer expired-access-token') {
            await route.fulfill({
              status: 401,
              contentType: 'application/json',
              body: JSON.stringify({
                errors: [{ message: 'Token expired' }]
              })
            });
          } else {
            // Second call succeeds with new token
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
          }
        } else {
          await route.continue();
        }
      });
      
      // Navigate to protected route
      await page.goto('/dashboard');
      
      // Should eventually load dashboard after token refresh
      await expect(page).toHaveURL(/.*\/dashboard.*/);
      
      // Verify new tokens are stored
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refreshToken'));
      expect(accessToken).toBe('new-access-token');
      expect(refreshToken).toBe('new-refresh-token');
    });

    test('should handle failed token refresh', async ({ page }) => {
      // Set up tokens in localStorage
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'expired-access-token');
        localStorage.setItem('refreshToken', 'invalid-refresh-token');
      });
      
      // Mock the GraphQL response for failed token refresh
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('refreshToken')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                refreshToken: {
                  success: false,
                  errors: ['Invalid refresh token']
                }
              }
            })
          });
        } else if (postData?.includes('currentUser')) {
          await route.fulfill({
            status: 401,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'Token expired' }]
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Navigate to protected route
      await page.goto('/dashboard');
      
      // Should redirect to login after failed refresh
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
      
      // Verify tokens are cleared
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      const refreshToken = await page.evaluate(() => localStorage.getItem('refreshToken'));
      expect(accessToken).toBeNull();
      expect(refreshToken).toBeNull();
    });

    test('should handle token refresh when no refresh token exists', async ({ page }) => {
      // Set up only access token (no refresh token)
      await page.evaluate(() => {
        localStorage.setItem('accessToken', 'expired-access-token');
      });
      
      // Mock the GraphQL response for expired token
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('currentUser')) {
          await route.fulfill({
            status: 401,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'Token expired' }]
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Navigate to protected route
      await page.goto('/dashboard');
      
      // Should redirect to login (no refresh token available)
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
      
      // Verify tokens are cleared
      const accessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
      expect(accessToken).toBeNull();
    });
  });

  test.describe('Keycloak Integration Flow', () => {
    test('should handle full OAuth flow simulation', async ({ page }) => {
      // Start at login page
      await page.goto('/auth/login');
      
      // Mock OAuth initiation
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
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Should redirect to Keycloak (we'll simulate this)
      await expect(page).toHaveURL(/.*localhost:8080.*/);
      
      // Verify OAuth state is stored
      const oauthState = await page.evaluate(() => sessionStorage.getItem('oauth_state'));
      expect(oauthState).toBe('mock-state-123');
    });

    test('should handle Keycloak unavailable', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Mock OAuth initiation failure (Keycloak unavailable)
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('initiateLogin')) {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'Keycloak service unavailable' }]
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Should show error message
      await expect(page.locator('simple-snack-bar')).toContainText('Network error occurred');
      
      // Should stay on login page
      await expect(page).toHaveURL(/.*\/auth\/login.*/);
    });

    test('should handle OAuth timeout', async ({ page }) => {
      await page.goto('/auth/login');
      
      // Mock OAuth initiation with timeout
      await page.route('**/graphql', async route => {
        const request = route.request();
        const postData = request.postData();
        
        if (postData?.includes('initiateLogin')) {
          // Simulate timeout by delaying response
          await new Promise(resolve => setTimeout(resolve, 31000)); // 31 seconds
          await route.fulfill({
            status: 408,
            contentType: 'application/json',
            body: JSON.stringify({
              errors: [{ message: 'Request timeout' }]
            })
          });
        } else {
          await route.continue();
        }
      });
      
      // Click OAuth login button
      await page.locator('button:has-text("Mit Keycloak anmelden")').click();
      
      // Should show timeout error
      await expect(page.locator('simple-snack-bar')).toContainText('Network error occurred');
    });
  });
});