import { createFeatureSelector, createSelector } from '@ngrx/store';

import { HealthState } from './health.models';
import { healthFeatureKey } from './health.reducer';

export const selectHealthState = createFeatureSelector<HealthState>(healthFeatureKey);

export const selectHealthData = createSelector(
  selectHealthState,
  (state) => state.data
);

export const selectHealthLoading = createSelector(
  selectHealthState,
  (state) => state.loading
);

export const selectHealthError = createSelector(
  selectHealthState,
  (state) => state.error
);
