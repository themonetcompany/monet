import { createReducer, on } from '@ngrx/store';

import { HealthActions } from './health.actions';
import { HealthState } from './health.models';

export const healthFeatureKey = 'health';

export const initialHealthState: HealthState = {
  data: null,
  loading: false,
  error: null
};

export const healthReducer = createReducer(
  initialHealthState,
  on(HealthActions.loadHealth, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(HealthActions.loadHealthSuccess, (state, { data }) => ({
    ...state,
    data,
    loading: false
  })),
  on(HealthActions.loadHealthFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
