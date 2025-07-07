import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Basic Accessibility Tests', () => {
  test.describe('Error Pages', () => {
    test('should have no accessibility violations on 404 page', async ({ page }) => {
      await page.goto('/non-existent-page');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa', 'wcag21aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have proper heading hierarchy on 404 page', async ({ page }) => {
      await page.goto('/non-existent-page');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['heading-order', 'page-has-heading-one'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have accessible navigation links on 404 page', async ({ page }) => {
      await page.goto('/non-existent-page');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['link-name', 'button-name'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('Developer Tools', () => {
    test('should have no accessibility violations on health dashboard', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have accessible buttons on health dashboard', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      // Check refresh button accessibility
      const refreshButton = page.locator('button:has-text("Aktualisieren")');
      if (await refreshButton.count() > 0) {
        await expect(refreshButton).toHaveAttribute('aria-label');
      }
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['button-name', 'aria-valid-attr-value'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have no accessibility violations on GraphQL playground', async ({ page }) => {
      await page.goto('/developer-tools/graphql-playground');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have accessible iframe on GraphQL playground', async ({ page }) => {
      await page.goto('/developer-tools/graphql-playground');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['frame-title', 'frame-title-unique'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('Color Contrast', () => {
    test('should meet WCAG AA color contrast requirements', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2aa'])
        .withRules(['color-contrast'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should not rely solely on color for information', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['color-contrast', 'link-in-text-block'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('Keyboard Navigation', () => {
    test('should support keyboard navigation', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      // Test tab navigation
      await page.keyboard.press('Tab');
      const firstFocusable = page.locator(':focus');
      if (await firstFocusable.count() > 0) {
        await expect(firstFocusable).toBeVisible();
      }
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['focus-order-semantics'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have accessible focus indicators', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['focus-order-semantics'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('ARIA and Semantic HTML', () => {
    test('should have proper ARIA attributes', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules([
          'aria-valid-attr',
          'aria-valid-attr-value',
          'aria-required-attr',
          'aria-hidden-focus'
        ])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have meaningful button and link names', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['button-name', 'link-name'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have proper form labels', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withRules(['label', 'aria-input-field-name'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('Mobile and Responsive', () => {
    test('should be accessible on mobile devices', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/developer-tools/health-dashboard');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have adequate touch target sizes on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/developer-tools/health-dashboard');
      
      // Check main interactive elements (buttons) have adequate size
      const buttons = page.locator('button');
      const buttonCount = await buttons.count();
      
      // Check at least some buttons meet the minimum size requirement
      let adequateSizeCount = 0;
      for (let i = 0; i < Math.min(buttonCount, 5); i++) {
        const button = buttons.nth(i);
        const boundingBox = await button.boundingBox();
        if (boundingBox && boundingBox.height >= 44 && boundingBox.width >= 44) {
          adequateSizeCount++;
        }
      }
      
      // At least some buttons should meet the size requirement
      if (buttonCount > 0) {
        expect(adequateSizeCount).toBeGreaterThan(0);
      }
    });

    test('should work with browser zoom', async ({ page }) => {
      await page.goto('/developer-tools/health-dashboard');
      
      // Test with 150% zoom
      await page.evaluate(() => {
        document.body.style.zoom = '1.5';
      });

      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('Progressive Enhancement', () => {
    test('should work without JavaScript for basic content', async ({ browser }) => {
      // Create a new context with JavaScript disabled
      const context = await browser.newContext({ javaScriptEnabled: false });
      const page = await context.newPage();
      await page.goto('/non-existent-page');

      // Basic content should still be accessible
      const heading = page.locator('h1, h2').first();
      if (await heading.count() > 0) {
        await expect(heading).toBeVisible();
      }
      
      // Links should still work
      const links = page.locator('a[href]');
      const linkCount = await links.count();
      
      for (let i = 0; i < Math.min(linkCount, 3); i++) {
        const link = links.nth(i);
        const href = await link.getAttribute('href');
        expect(href).toBeTruthy();
      }
      
      await context.close();
    });
  });

  test.describe('Motion and Animation', () => {
    test('should respect reduced motion preferences', async ({ page }) => {
      await page.emulateMedia({ reducedMotion: 'reduce' });
      await page.goto('/developer-tools/health-dashboard');
      
      // Check that critical content is still accessible
      const headings = page.locator('h1, h2, h3');
      const headingCount = await headings.count();
      
      expect(headingCount).toBeGreaterThan(0);
      
      // Animation-related accessibility should still pass
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });

  test.describe('Error State Accessibility', () => {
    test('should handle error states accessibly on health dashboard', async ({ page }) => {
      // Mock error response to test error state
      await page.route('/api/health', route => route.abort('failed'));
      await page.goto('/developer-tools/health-dashboard');
      
      // Error states should still be accessible
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa'])
        .analyze();

      expect(accessibilityScanResults.violations).toEqual([]);
    });
  });
});