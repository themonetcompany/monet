import { createAction, props } from '@ngrx/store';

import { AccountBalanceReadModel, TransactionReadModel } from './dashboard.models';

const loadDashboard = createAction('[Dashboard] Load');
const loadDashboardSuccess = createAction(
  '[Dashboard] Load Success',
  props<{ accountBalances: AccountBalanceReadModel[]; transactions: TransactionReadModel[] }>()
);
const loadDashboardFailure = createAction(
  '[Dashboard] Load Failure',
  props<{ error: string }>()
);

export const DashboardActions = {
  loadDashboard,
  loadDashboardSuccess,
  loadDashboardFailure
};
