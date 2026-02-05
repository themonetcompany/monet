import {
  ApplicationConfig,
  LOCALE_ID,
  isDevMode,
  provideBrowserGlobalErrorListeners
} from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { routes } from './app.routes';
import { DashboardEffects } from './state/dashboard/dashboard.effects';
import { dashboardFeatureKey, dashboardReducer } from './state/dashboard/dashboard.reducer';
import { HealthEffects } from './state/health/health.effects';
import { healthFeatureKey, healthReducer } from './state/health/health.reducer';
import { VersionEffects } from './state/version/version.effects';
import { versionFeatureKey, versionReducer } from './state/version/version.reducer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: LOCALE_ID, useValue: 'fr-FR' },
    provideRouter(routes),
    provideHttpClient(),
    provideStore({
      [dashboardFeatureKey]: dashboardReducer,
      [healthFeatureKey]: healthReducer,
      [versionFeatureKey]: versionReducer
    }),
    provideEffects(DashboardEffects, HealthEffects, VersionEffects),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode()
    })
  ]
};
