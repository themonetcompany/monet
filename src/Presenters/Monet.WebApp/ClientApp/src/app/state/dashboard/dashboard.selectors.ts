import { createFeatureSelector, createSelector } from '@ngrx/store';

import { DashboardState } from './dashboard.models';
import { dashboardFeatureKey } from './dashboard.reducer';

export const selectDashboardState = createFeatureSelector<DashboardState>(dashboardFeatureKey);

export const selectDashboardAccountBalances = createSelector(
  selectDashboardState,
  (state) => state.accountBalances
);

export const selectDashboardTransactions = createSelector(
  selectDashboardState,
  (state) => state.transactions
);

export const selectDashboardLoading = createSelector(
  selectDashboardState,
  (state) => state.loading
);

export const selectDashboardError = createSelector(
  selectDashboardState,
  (state) => state.error
);
