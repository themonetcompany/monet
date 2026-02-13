import { createReducer, on } from '@ngrx/store';

import { DashboardActions } from './dashboard.actions';
import { DashboardState } from './dashboard.models';

export const dashboardFeatureKey = 'dashboard';

export const initialDashboardState: DashboardState = {
  accountBalances: [],
  transactions: [],
  categories: [],
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
  on(DashboardActions.loadDashboardSuccess, (state, { accountBalances, transactions, categories }) => ({
    ...state,
    accountBalances,
    transactions: transactions.map((transaction) => {
      const resolvedCategory = categories.find((category) => category.categoryId === transaction.categoryId);
      return {
        ...transaction,
        categoryName: transaction.categoryName ?? resolvedCategory?.name ?? null
      };
    }),
    categories,
    loading: false
  })),
  on(DashboardActions.loadDashboardFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),
  on(DashboardActions.updateTransactionCategory, (state, { transactionId, categoryId }) => ({
    ...state,
    transactions: state.transactions.map((transaction) => {
      if (transaction.transactionId !== transactionId) {
        return transaction;
      }

      const category = state.categories.find((item) => item.categoryId === categoryId) ?? null;
      return {
        ...transaction,
        categoryId,
        categoryName: category?.name ?? null
      };
    }),
    error: null
  })),
  on(DashboardActions.updateTransactionCategorySuccess, (state) => ({
    ...state
  })),
  on(DashboardActions.updateTransactionCategoryFailure, (state, { transactionId, previousCategoryId, previousCategoryName, error }) => ({
    ...state,
    transactions: state.transactions.map((transaction) =>
      transaction.transactionId === transactionId
        ? {
            ...transaction,
            categoryId: previousCategoryId,
            categoryName: previousCategoryName
          }
        : transaction
    ),
    error
  }))
);
