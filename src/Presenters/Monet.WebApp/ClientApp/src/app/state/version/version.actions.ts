import { createAction, props } from '@ngrx/store';

import { VersionResponse } from './version.models';

const loadVersion = createAction('[Version] Load');
const loadVersionSuccess = createAction(
  '[Version] Load Success',
  props<{ data: VersionResponse }>()
);
const loadVersionFailure = createAction(
  '[Version] Load Failure',
  props<{ error: string }>()
);

export const VersionActions = {
  loadVersion,
  loadVersionSuccess,
  loadVersionFailure
};
