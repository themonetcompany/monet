import { createFeatureSelector, createSelector } from '@ngrx/store';

import type { DashboardState } from './dashboard.models';
import { dashboardFeatureKey } from './dashboard.reducer';

const selectDashboardState = createFeatureSelector<DashboardState>(dashboardFeatureKey);

export const selectDashboardLoading = createSelector(
  selectDashboardState,
  (state) => state.loading
);

export const selectDashboardError = createSelector(
  selectDashboardState,
  (state) => state.error
);

export const selectDashboardSummary = createSelector(
  selectDashboardState,
  (state) => state.summary
);

export const selectDashboardProjects = createSelector(
  selectDashboardState,
  (state) => state.projects
);

export const selectDashboardAreas = createSelector(
  selectDashboardState,
  (state) => state.areas
);

export const selectDashboardActivity = createSelector(
  selectDashboardState,
  (state) => state.recentActivity
);
