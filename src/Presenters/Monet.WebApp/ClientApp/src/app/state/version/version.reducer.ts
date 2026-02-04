import { createReducer, on } from '@ngrx/store';

import { VersionActions } from './version.actions';
import { VersionState } from './version.models';

export const versionFeatureKey = 'version';

export const initialVersionState: VersionState = {
  value: null,
  loading: false,
  error: null
};

export const versionReducer = createReducer(
  initialVersionState,
  on(VersionActions.loadVersion, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(VersionActions.loadVersionSuccess, (state, { data }) => ({
    ...state,
    value: data.version,
    loading: false
  })),
  on(VersionActions.loadVersionFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
