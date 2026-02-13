import { createAction, props } from '@ngrx/store';

import { AccountBalanceReadModel, TransactionCategoryReadModel, TransactionReadModel } from './dashboard.models';

const loadDashboard = createAction('[Dashboard] Load');
const loadDashboardSuccess = createAction(
  '[Dashboard] Load Success',
  props<{
    accountBalances: AccountBalanceReadModel[];
    transactions: TransactionReadModel[];
    categories: TransactionCategoryReadModel[];
  }>()
);
const loadDashboardFailure = createAction(
  '[Dashboard] Load Failure',
  props<{ error: string }>()
);
const updateTransactionCategory = createAction(
  '[Dashboard] Update Transaction Category',
  props<{
    transactionId: string;
    categoryId: string | null;
    previousCategoryId: string | null;
    previousCategoryName: string | null;
  }>()
);
const updateTransactionCategorySuccess = createAction('[Dashboard] Update Transaction Category Success');
const updateTransactionCategoryFailure = createAction(
  '[Dashboard] Update Transaction Category Failure',
  props<{
    transactionId: string;
    previousCategoryId: string | null;
    previousCategoryName: string | null;
    error: string;
  }>()
);

export const DashboardActions = {
  loadDashboard,
  loadDashboardSuccess,
  loadDashboardFailure,
  updateTransactionCategory,
  updateTransactionCategorySuccess,
  updateTransactionCategoryFailure,
};
