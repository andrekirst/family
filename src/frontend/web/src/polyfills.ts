/**
 * This file includes polyfills needed by Angular and is loaded before the app.
 * You can add your own extra polyfills to this file.
 *
 * This file is divided into 2 sections:
 *   1. Browser polyfills. These are applied before loading ZoneJS and are sorted by browsers.
 *   2. Application imports. Files imported after ZoneJS that should be loaded before your main
 *      file.
 *
 * The current setup is for so-called "evergreen" browsers; the last versions of browsers that
 * automatically update themselves. This includes recent versions of Safari, Chrome (including
 * Opera), Edge on the Chromium engine and Firefox.
 *
 * Learn more in https://angular.io/guide/browser-support
 */

/***************************************************************************************************
 * BROWSER POLYFILLS
 */

/**
 * URL.parse() polyfill for browsers that don't support it (like older Edge versions)
 * This method was added in more recent browser versions and is used by some dependencies
 */
(function() {
  'use strict';
  
  // Comprehensive URL.parse polyfill
  if (typeof URL !== 'undefined' && !(URL as any).parse) {
    console.log('[Polyfill] Adding URL.parse() support for browser compatibility');
    
    // Define URL.parse as a static method
    Object.defineProperty(URL, 'parse', {
      value: function(url: string, base?: string | URL): URL | null {
        try {
          return new URL(url, base);
        } catch (error) {
          // URL.parse should return null for invalid URLs, not throw
          return null;
        }
      },
      writable: true,
      configurable: true
    });
    
    console.log('[Polyfill] URL.parse() polyfill installed successfully');
  } else if (typeof URL !== 'undefined' && (URL as any).parse) {
    console.log('[Polyfill] URL.parse() is natively supported');
  } else {
    console.warn('[Polyfill] URL constructor not available');
  }
})();

/**
 * By default, zone.js will patch all possible macroTask and DomEvents
 * user can disable parts of macroTask/DomEvents patch by setting following flags
 * because those flags need to be set before `zone.js` being loaded, and webpack
 * will put import in the top of bundle, so user need to create a separate file
 * in this directory (for example: zone-flags.ts), and put the following flags
 * into that file, and then add the following code before importing zone.js.
 * import './zone-flags';
 *
 * The flags allowed in zone-flags.ts are listed here.
 *
 * The following flags will disable zone.js patches for ALL browsers (IE and others)
 * (window as any).__Zone_disable_requestAnimationFrame = true; // disable patch requestAnimationFrame
 * (window as any).__Zone_disable_on_property = true; // disable patch onProperty such as onclick
 * (window as any).__zone_symbol__UNPATCHED_EVENTS = ['scroll', 'mousemove']; // disable patch specified events
 *
 * The following code will disable zone.js patches for all IE browsers, this could be added
 * in zone-flags.ts and imported before zone.js loading
 */

/***************************************************************************************************
 * Zone JS is required by default for Angular itself.
 * NOTE: URL.parse polyfill must be loaded BEFORE zone.js to ensure it's available early
 */
import 'zone.js';  // Included with Angular CLI.

/***************************************************************************************************
 * APPLICATION IMPORTS
 */