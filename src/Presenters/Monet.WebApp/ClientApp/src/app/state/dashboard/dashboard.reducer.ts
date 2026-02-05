import { createReducer, on } from '@ngrx/store';

import { DashboardActions } from './dashboard.actions';
import type { DashboardState } from './dashboard.models';

export const dashboardFeatureKey = 'dashboard';

const initialState: DashboardState = {
  loading: false,
  error: null,
  summary: null,
  projects: [],
  areas: [],
  recentActivity: []
};

export const dashboardReducer = createReducer(
  initialState,
  on(DashboardActions.loadDashboard, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(DashboardActions.loadDashboardSuccess, (state, payload) => ({
    ...state,
    loading: false,
    error: null,
    summary: payload.summary,
    projects: payload.projects,
    areas: payload.areas,
    recentActivity: payload.recentActivity
  })),
  on(DashboardActions.loadDashboardFailure, (state, payload) => ({
    ...state,
    loading: false,
    error: payload.error
  }))
);
