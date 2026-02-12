import { createReducer, on } from '@ngrx/store';

import { DashboardActions } from './dashboard.actions';
import { DashboardState } from './dashboard.models';

export const dashboardFeatureKey = 'dashboard';

export const initialDashboardState: DashboardState = {
  accountBalances: [],
  transactions: [],
  loading: false,
  error: null
};

export const dashboardReducer = createReducer(
  initialDashboardState,
  on(DashboardActions.loadDashboard, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(DashboardActions.loadDashboardSuccess, (state, { accountBalances, transactions }) => ({
    ...state,
    accountBalances,
    transactions,
    loading: false
  })),
  on(DashboardActions.loadDashboardFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
