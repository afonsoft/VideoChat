/***************************************************************************************************
 * Load $ivyLoader and configuration
 */
(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ?
        factory((global as any).require('@angular/core'), (global as any).require('@angular/platform-browser-dynamic'), (global as any).require('@angular/common'), (global as any).require('@angular/forms'), (global as any).require('@abp/ng.core'), (global as any).require('@abp/ng.theme.shared')) :
        factory((global as any).require('@angular/core'), (global as any).require('@angular/platform-browser-dynamic'), (global as any).require('@angular/common'), (global as any).require('@angular/forms'), (global as any).require('@abp/ng.core'), (global as any).require('@abp/ng.theme.shared'));
})(typeof window !== 'undefined' ? window : this, () => {
    'use strict';
});
