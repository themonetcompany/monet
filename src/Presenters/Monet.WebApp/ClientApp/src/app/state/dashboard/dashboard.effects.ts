import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, forkJoin, map, mergeMap, of, switchMap } from 'rxjs';

import { DashboardActions } from './dashboard.actions';
import { AccountBalanceReadModel, TransactionCategoryReadModel, TransactionReadModel } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardEffects {
  private readonly actions$ = inject(Actions);
  private readonly http = inject(HttpClient);

  readonly loadDashboard$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDashboard),
      switchMap(() =>
        forkJoin({
          accountBalances: this.http.get<AccountBalanceReadModel[]>('/api/accounts/balances'),
          transactions: this.http.get<TransactionReadModel[]>('/api/transactions'),
          categories: this.http.get<TransactionCategoryReadModel[]>('/api/transactions/categories')
        }).pipe(
          map(({ accountBalances, transactions, categories }) =>
            DashboardActions.loadDashboardSuccess({
              accountBalances: [...accountBalances].sort((left, right) =>
                left.accountNumber.localeCompare(right.accountNumber)
              ),
              transactions: [...transactions].sort(
                (left, right) => new Date(right.date).getTime() - new Date(left.date).getTime()
              ),
              categories: [...categories].sort((left, right) => {
                if (left.flowType !== right.flowType) {
                  return left.flowType.localeCompare(right.flowType);
                }

                return left.name.localeCompare(right.name);
              })
            })
          ),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardFailure({
                error: this.formatError(error)
              })
            )
          )
        )
      )
    )
  );

  readonly updateTransactionCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.updateTransactionCategory),
      mergeMap(({ transactionId, categoryId, previousCategoryId, previousCategoryName }) =>
        this.http
          .put<void>(`/api/transactions/${encodeURIComponent(transactionId)}/category`, {
            categoryId
          })
          .pipe(
            map(() => DashboardActions.updateTransactionCategorySuccess()),
            catchError((error) =>
              of(
                DashboardActions.updateTransactionCategoryFailure({
                  transactionId,
                  previousCategoryId,
                  previousCategoryName,
                  error: this.formatError(error)
                })
              )
            )
          )
      )
    )
  );

  private formatError(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Vous devez être authentifié pour voir le dashboard.';
    }

    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string') {
        return error.error;
      }

      if (error.message) {
        return error.message;
      }

      return `HTTP ${error.status}`;
    }

    if (error instanceof Error) {
      return error.message;
    }

    return 'Impossible de charger les données du dashboard.';
  }
}
