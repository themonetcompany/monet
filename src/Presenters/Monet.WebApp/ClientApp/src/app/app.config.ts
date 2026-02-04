import {
  ApplicationConfig,
  isDevMode,
  provideBrowserGlobalErrorListeners
} from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { routes } from './app.routes';
import { HealthEffects } from './state/health/health.effects';
import { healthFeatureKey, healthReducer } from './state/health/health.reducer';
import { VersionEffects } from './state/version/version.effects';
import { versionFeatureKey, versionReducer } from './state/version/version.reducer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideStore({
      [healthFeatureKey]: healthReducer,
      [versionFeatureKey]: versionReducer
    }),
    provideEffects(HealthEffects, VersionEffects),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode()
    })
  ]
};
