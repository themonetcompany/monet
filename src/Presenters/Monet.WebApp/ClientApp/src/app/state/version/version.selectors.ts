import { createFeatureSelector, createSelector } from '@ngrx/store';

import { versionFeatureKey } from './version.reducer';
import { VersionState } from './version.models';

export const selectVersionState = createFeatureSelector<VersionState>(versionFeatureKey);

export const selectVersionValue = createSelector(
  selectVersionState,
  (state) => state.value
);

export const selectVersionLoading = createSelector(
  selectVersionState,
  (state) => state.loading
);

export const selectVersionError = createSelector(
  selectVersionState,
  (state) => state.error
);
