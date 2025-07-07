import { test, expect } from '@playwright/test';

test.describe('Family Creation Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to first-time user page
    await page.goto('/onboarding/first-time-user');
  });

  test('should display first-time user onboarding page', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('Willkommen');
    await expect(page.locator('[data-testid="create-family-btn"]')).toBeVisible();
    await expect(page.locator('[data-testid="skip-btn"]')).toBeVisible();
  });

  test('should open family creation modal when create button is clicked', async ({ page }) => {
    // Click create family button
    await page.click('[data-testid="create-family-btn"]');

    // Modal should be visible
    await expect(page.locator('[role="dialog"]')).toBeVisible();
    await expect(page.locator('#modal-title')).toContainText('Familie erstellen');
    await expect(page.locator('#familyName')).toBeVisible();
  });

  test('should validate family name input', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');

    // Try to submit empty form
    await page.click('button[type="submit"]');
    
    // Should show validation error
    await expect(page.locator('#familyName-error')).toContainText('erforderlich');

    // Enter invalid name (too short)
    await page.fill('#familyName', 'A');
    await page.blur('#familyName');
    await expect(page.locator('#familyName-error')).toContainText('mindestens');

    // Enter invalid characters
    await page.fill('#familyName', 'Family123!');
    await page.blur('#familyName');
    await expect(page.locator('#familyName-error')).toContainText('ungültige Zeichen');

    // Enter valid name
    await page.fill('#familyName', 'Müller Familie');
    await expect(page.locator('#familyName-error')).not.toBeVisible();
  });

  test('should close modal when cancel is clicked', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');
    await expect(page.locator('[role="dialog"]')).toBeVisible();

    // Click cancel
    await page.click('button:has-text("Abbrechen")');
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();
  });

  test('should close modal when close button is clicked', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');
    await expect(page.locator('[role="dialog"]')).toBeVisible();

    // Click close button
    await page.click('.modal-close-btn');
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();
  });

  test('should close modal when overlay is clicked', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');
    await expect(page.locator('[role="dialog"]')).toBeVisible();

    // Click overlay (outside modal content)
    await page.click('.modal-overlay', { position: { x: 10, y: 10 } });
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();
  });

  test('should handle escape key to close modal', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');
    await expect(page.locator('[role="dialog"]')).toBeVisible();

    // Press escape key
    await page.keyboard.press('Escape');
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();
  });

  test('should focus on family name input when modal opens', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');

    // Family name input should be focused
    await expect(page.locator('#familyName')).toBeFocused();
  });

  test('should submit valid family creation', async ({ page }) => {
    // Mock successful family creation
    await page.route('/graphql', async route => {
      const request = route.request();
      const postData = request.postData();
      
      if (postData?.includes('createFamily')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              createFamily: {
                success: true,
                family: {
                  id: '123',
                  name: 'Test Familie',
                  ownerId: 'user123'
                }
              }
            }
          })
        });
      } else {
        await route.continue();
      }
    });

    // Open modal and fill form
    await page.click('[data-testid="create-family-btn"]');
    await page.fill('#familyName', 'Test Familie');
    
    // Submit form
    await page.click('button[type="submit"]');

    // Should redirect to dashboard with success message
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.url()).toContain('familyCreated=true');
  });

  test('should handle family creation error', async ({ page }) => {
    // Mock failed family creation
    await page.route('/graphql', async route => {
      const request = route.request();
      const postData = request.postData();
      
      if (postData?.includes('createFamily')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              createFamily: {
                success: false,
                errorMessage: 'Familienname bereits vergeben'
              }
            }
          })
        });
      } else {
        await route.continue();
      }
    });

    // Open modal and fill form
    await page.click('[data-testid="create-family-btn"]');
    await page.fill('#familyName', 'Existing Familie');
    
    // Submit form
    await page.click('button[type="submit"]');

    // Should show error message
    await expect(page.locator('[role="alert"]')).toContainText('Familienname bereits vergeben');
    
    // Modal should remain open
    await expect(page.locator('[role="dialog"]')).toBeVisible();
  });

  test('should show loading state during submission', async ({ page }) => {
    // Mock slow response
    await page.route('/graphql', async route => {
      const request = route.request();
      const postData = request.postData();
      
      if (postData?.includes('createFamily')) {
        // Delay response
        await new Promise(resolve => setTimeout(resolve, 1000));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              createFamily: {
                success: true,
                family: { id: '123', name: 'Test Familie' }
              }
            }
          })
        });
      } else {
        await route.continue();
      }
    });

    // Open modal and fill form
    await page.click('[data-testid="create-family-btn"]');
    await page.fill('#familyName', 'Test Familie');
    
    // Submit form
    await page.click('button[type="submit"]');

    // Should show loading state
    await expect(page.locator('.animate-spin')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeDisabled();
    await expect(page.locator('button[type="submit"]')).toContainText('Erstelle');

    // Wait for completion
    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('should navigate to dashboard when skip is clicked', async ({ page }) => {
    // Click skip button
    await page.click('[data-testid="skip-btn"]');

    // Should redirect to dashboard
    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('should be accessible', async ({ page }) => {
    // Open modal
    await page.click('[data-testid="create-family-btn"]');

    // Check ARIA attributes
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toHaveAttribute('aria-modal', 'true');
    await expect(modal).toHaveAttribute('aria-labelledby', 'modal-title');
    await expect(modal).toHaveAttribute('aria-describedby', 'modal-description');

    // Check form accessibility
    const input = page.locator('#familyName');
    await expect(input).toHaveAttribute('aria-required', 'true');
    await expect(input).toHaveAttribute('required');

    // Test keyboard navigation
    await page.keyboard.press('Tab');
    await expect(input).toBeFocused();
    
    await page.keyboard.press('Tab');
    await expect(page.locator('button:has-text("Abbrechen")')).toBeFocused();
    
    await page.keyboard.press('Tab');
    await expect(page.locator('button[type="submit"]')).toBeFocused();
  });

  test('should handle network errors gracefully', async ({ page }) => {
    // Mock network error
    await page.route('/graphql', route => route.abort('failed'));

    // Open modal and fill form
    await page.click('[data-testid="create-family-btn"]');
    await page.fill('#familyName', 'Test Familie');
    
    // Submit form
    await page.click('button[type="submit"]');

    // Should show error message
    await expect(page.locator('[role="alert"]')).toContainText('Fehler');
    
    // Modal should remain open
    await expect(page.locator('[role="dialog"]')).toBeVisible();
  });
});