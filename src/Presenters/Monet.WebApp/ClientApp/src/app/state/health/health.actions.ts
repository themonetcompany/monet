import { createAction, props } from '@ngrx/store';

import { HealthStatus } from './health.models';

const loadHealth = createAction('[Health] Load');
const loadHealthSuccess = createAction(
  '[Health] Load Success',
  props<{ data: HealthStatus }>()
);
const loadHealthFailure = createAction(
  '[Health] Load Failure',
  props<{ error: string }>()
);

export const HealthActions = {
  loadHealth,
  loadHealthSuccess,
  loadHealthFailure
};
